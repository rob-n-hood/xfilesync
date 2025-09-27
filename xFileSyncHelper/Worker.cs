using Amazon.Runtime.Internal.Transform;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;

//namespace xfilesync // <-- Make sure this is your project's namespace
//{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly string _watchPath;
        private readonly string _bucketName;
        private readonly FileSystemWatcher _watcher;
        private readonly ConcurrentDictionary<string, Timer> _debounceTimers = new();
        private readonly IAmazonS3 _s3Client;

        private readonly IHostApplicationLifetime _appLifetime ;
        private const string PipeName = "S3SyncServicePipe";

        public Worker(ILogger<Worker> logger, IConfiguration config, IAmazonS3 s3Client, IHostApplicationLifetime appLifetime)
        {
            _appLifetime = appLifetime; // Injected service to control shutdown
            _logger = logger;
            _s3Client = s3Client; // Inject IAmazonS3
            _watchPath = config.GetValue<string>("SyncSettings:WatchPath") ?? throw new InvalidOperationException("WatchPath is not set.");
            _bucketName = config.GetValue<string>("SyncSettings:BucketName") ?? throw new InvalidOperationException("BucketName is not set.");
            _watcher = new FileSystemWatcher();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Sync Service starting at: {time}", DateTimeOffset.Now);

            // Start a background task to listen for commands from the tray app.
            _ = Task.Run(() => ListenForCommands(stoppingToken), stoppingToken);

            try
            {
                if (!Directory.Exists(_watchPath))
                {
                Log.Information("Watch path {path} does not exist. Creating it.", _watchPath);
                //_logger.LogWarning("Watch path {path} does not exist. Creating it.", _watchPath);
                    Directory.CreateDirectory(_watchPath);
                }
                InitializeWatcher();
            }
            catch (Exception ex)
            {
            Log.Error(ex, "An error occurred during service startup. The service will stop.");
            //_logger.LogError(ex, "An error occurred during service startup. The service will stop.");
                return;
            }
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }


        /// <summary>
        /// Listens for commands from the tray application via a named pipe.
        /// </summary>
        private async Task ListenForCommands(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    // Create a server stream to listen for a client connection.
                    using (var server = new NamedPipeServerStream(PipeName, PipeDirection.In))
                    {
                        await server.WaitForConnectionAsync(token);
                        using (var reader = new StreamReader(server))
                        {
                            var command = await reader.ReadLineAsync();
                            if (command != null)
                            {
                                HandleCommand(command);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error in IPC command listener pipe.");
                    //_logger.LogError(ex, "Error in IPC command listener pipe.");
                }
            }
        }

        /// <summary>
        /// Acts on a command received from the IPC listener.
        /// </summary>
        private void HandleCommand(string command)
        {
             Log.Information("Received command from tray app: {command}", command);
            //_logger.LogInformation("Received command from tray app: {command}", command);
            switch (command)
            {
                case "PAUSE_SYNC":
                    _watcher.EnableRaisingEvents = false;
                    Log.Information("File watcher has been paused.");
                    //_logger.LogInformation("File watcher has been paused.");
                    break;

                case "RESUME_SYNC":
                    _watcher.EnableRaisingEvents = true;
                    Log.Information("File watcher has been resumed.");
                    //_logger.LogInformation("File watcher has been resumed.");
                    break;

                case "FORCE_SYNC_ALL":
                    Log.Information("--- Starting a forced full sync of the directory ---");
                    //_logger.LogInformation("--- Starting a forced full sync of the directory ---");
                    ForceFullSync();
                    break;
                case "RELOAD_CONFIG":
                    Log.Information("Reload command received. Service will shut down to apply new settings.");
                    //_logger.LogInformation("Reload command received. Service will shut down to apply new settings.");
                    // This gracefully stops the service. The Windows Service Manager will then restart it.
                    _appLifetime.StopApplication();
                    break;
                case "SHUTDOWN":
                    Log.Information("Shutdown command received. Service is stopping.");
                    //Log.Information("Shutdown command received. Service is stopping.");
                    _appLifetime.StopApplication();
                    break;
            }
        }

        private void ForceFullSync()
        {
            // Run the sync operation on a background thread to not block the command listener.
            Task.Run(() =>
            {
                try
                {
                    // 1. Get all directories and create them on the S3 server.
                    var allDirectories = Directory.GetDirectories(_watchPath, "*", SearchOption.AllDirectories);
                    foreach (var dir in allDirectories)
                    {
                        CreateFolderInS3(dir);
                    }
                    // Also create the root folder itself
                    //CreateFolderInS3(_watchPath); 


                    // 2. Get all files and upload them to the S3 server.
                    var allFiles = Directory.GetFiles(_watchPath, "*.*", SearchOption.AllDirectories);
                    Log.Information("--- Uploading {FileCount} files to S3 ---", allFiles.Length);
                    //_logger.LogInformation("Found {FileCount} files to potentially upload.", allFiles.Length);
                    foreach (var file in allFiles)
                    {
                        // Using QueueFileUpload will debounce, but for a full sync, direct upload is fine.
                        UploadFileToS3(file);
                    }

                    Log.Information("--- Full Sync Completed ---");
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "An error occurred during the full sync operation.");
                }
            });
        }

        private void InitializeWatcher()
        {
            _watcher.Path = _watchPath;
            _watcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.LastWrite;
            _watcher.IncludeSubdirectories = true;

            _watcher.Created += OnFileCreated;
            _watcher.Changed += OnFileChanged;
            _watcher.Deleted += OnFileDeleted;
            _watcher.Renamed += OnFileRenamed;

            _watcher.EnableRaisingEvents = true;
            Log.Information("Now watching directory: {path}", _watchPath);
        }

        private void OnFileCreated(object sender, FileSystemEventArgs e)
        {
            if (Directory.Exists(e.FullPath))
            {
            Log.Information("Folder created: {path}", e.FullPath);
                      CreateFolderInS3(e.FullPath);
            }
            else
            {
            Log.Information("File created: {path}", e.FullPath);
      
                QueueFileUpload(e.FullPath);
            }
        }

        private void OnFileChanged(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType != WatcherChangeTypes.Changed) return;
            QueueFileUpload(e.FullPath);
        }

        private void OnFileDeleted(object sender, FileSystemEventArgs e)
        {
            Log.Information($"{e.FullPath} was deleted");
            //_logger.LogInformation("Item deleted: {path}", e.FullPath);
            if (_debounceTimers.TryRemove(e.FullPath, out var timer))
            {
                timer.Dispose();
                Log.Information("Cancelled pending upload for deleted file: {path}", e.FullPath);
            //_logger.LogInformation("Cancelled pending upload for deleted file: {path}", e.FullPath);
            }
            DeleteFromS3(e.FullPath);
        }

        private void OnFileRenamed(object sender, RenamedEventArgs e)
        {
            if (Directory.Exists(e.FullPath))
            {
                Log.Information("Folder renamed: {oldPath} to {newPath}", e.OldFullPath, e.FullPath);
                RenameFolderInS3(e.OldFullPath, e.FullPath);
            }
            else
            {
                Log.Information("File renamed: {oldPath} to {newPath}", e.OldFullPath, e.FullPath);
                if (_debounceTimers.TryRemove(e.OldFullPath, out var timer))
                {
                    timer.Dispose();
                }
                DeleteFromS3(e.OldFullPath);
                QueueFileUpload(e.FullPath);
            }
        }

        private void QueueFileUpload(string filePath)
        {
            Log.Information("Debouncing change for: {path}", filePath);
            if (_debounceTimers.TryGetValue(filePath, out var timer))
            {
                timer.Change(1000, Timeout.Infinite);
                return;
            }
            timer = new Timer(state =>
            {
                Log.Information("Debounce timer elapsed. Uploading: {path}", filePath);
                UploadFileToS3(filePath);
                if (_debounceTimers.TryRemove(filePath, out var t))
                {
                    t.Dispose();
                }
            }, null, 1000, Timeout.Infinite);
            _debounceTimers.TryAdd(filePath, timer);
        }

        #region S3 handling

        private async void CreateFolderInS3(string fullPath)
        {
            try
            {
                string s3Key = fullPath.Replace(_watchPath + Path.DirectorySeparatorChar, "").Replace(Path.DirectorySeparatorChar, '/') + "/";
                _logger.LogInformation("Creating folder object {key} in bucket {bucket}", s3Key, _bucketName);

                var putRequest = new PutObjectRequest
                {
                    BucketName = _bucketName,
                    Key = s3Key,
                    ContentBody = string.Empty
                };
                await _s3Client.PutObjectAsync(putRequest);
                _logger.LogInformation("Successfully created folder {key}", s3Key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating folder for path {path}", fullPath);
            }
        }


        private async void UploadFileToS3(string fullPath)
        {
            try
            {
                if (File.GetAttributes(fullPath).HasFlag(FileAttributes.Directory)) return;
                string s3Key = fullPath.Replace(_watchPath + Path.DirectorySeparatorChar, "").Replace(Path.DirectorySeparatorChar, '/');
                _logger.LogInformation("Uploading {key} to bucket {bucket}", s3Key, _bucketName);

                var transferUtility = new TransferUtility(_s3Client);
                await transferUtility.UploadAsync(fullPath, _bucketName, s3Key);
                _logger.LogInformation("Successfully uploaded {key}", s3Key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file {path}", fullPath);
            }
        }

        private async void DeleteFromS3(string fullPath)
        {
            try
            {
                string fileKey = fullPath.Replace(_watchPath + Path.DirectorySeparatorChar, "").Replace(Path.DirectorySeparatorChar, '/');
                string folderPrefix = fileKey + "/";
                _logger.LogInformation("Deleting item '{key}' from bucket '{bucket}'", fileKey, _bucketName);

                // First, gather all keys that need to be deleted.
                var keysToDelete = new List<string> { fileKey };
                var listRequest = new ListObjectsV2Request { BucketName = _bucketName, Prefix = folderPrefix };
                ListObjectsV2Response listResponse;
                do
                {
                    listResponse = await _s3Client.ListObjectsV2Async(listRequest);
                    if (listResponse.S3Objects != null)
                    {
                        // Add the keys from the folder listing
                        keysToDelete.AddRange(listResponse.S3Objects.Select(o => o.Key));
                    }
                    listRequest.ContinuationToken = listResponse.NextContinuationToken;
                } while (listResponse.IsTruncated == true);

                // --- THIS IS THE CHANGE ---
                // Loop through the list and delete each object individually.
                if (keysToDelete.Count > 0)
                {
                    _logger.LogInformation("Found {count} object(s) to delete individually.", keysToDelete.Count);
                    foreach (var key in keysToDelete)
                    {
                        var deleteRequest = new DeleteObjectRequest
                        {
                            BucketName = _bucketName,
                            Key = key
                        };
                        await _s3Client.DeleteObjectAsync(deleteRequest);
                    }
                }
                // --- END CHANGE ---

                _logger.LogInformation("Successfully processed delete request for '{key}'", fileKey);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting item for path '{path}' from S3", fullPath);
            }
        }

        private async void RenameFolderInS3(string oldFullPath, string newFullPath)
        {
            try
            {
                string oldPrefix = oldFullPath.Replace(_watchPath + Path.DirectorySeparatorChar, "").Replace(Path.DirectorySeparatorChar, '/') + "/";
                string newPrefix = newFullPath.Replace(_watchPath + Path.DirectorySeparatorChar, "").Replace(Path.DirectorySeparatorChar, '/') + "/";
                _logger.LogInformation("Renaming folder (download/upload method) from {oldPrefix} to {newPrefix}", oldPrefix, newPrefix);

                var listRequest = new ListObjectsV2Request { BucketName = _bucketName, Prefix = oldPrefix };
                var objectsToMove = new List<S3Object>();
                ListObjectsV2Response listResponse;
                do
                {
                    listResponse = await _s3Client.ListObjectsV2Async(listRequest);
                    objectsToMove.AddRange(listResponse.S3Objects);
                    listRequest.ContinuationToken = listResponse.NextContinuationToken;
                } while (listResponse.IsTruncated == true);

                foreach (var obj in objectsToMove)
                {
                    string newKey = newPrefix + obj.Key.Substring(oldPrefix.Length);
                    using (var getResponse = await _s3Client.GetObjectAsync(_bucketName, obj.Key))
                    using (var memoryStream = new MemoryStream())
                    {
                        await getResponse.ResponseStream.CopyToAsync(memoryStream);
                        memoryStream.Position = 0;
                        var putRequest = new PutObjectRequest { BucketName = _bucketName, Key = newKey, InputStream = memoryStream };
                        await _s3Client.PutObjectAsync(putRequest);
                    }
                }

                //if (objectsToMove.Any())
                //{
                //    var deleteRequest = new DeleteObjectsRequest { BucketName = _bucketName, Objects = objectsToMove.Select(o => new KeyVersion { Key = o.Key }).ToList() };
                //    await _s3Client.DeleteObjectsAsync(deleteRequest);
                //}

                if (objectsToMove.Any())
                {
                    // Loop through each object that needs to be moved/deleted.
                    foreach (var s3Object in objectsToMove)
                    {
                        // Create a request for a single object deletion.
                        var deleteRequest = new DeleteObjectRequest
                        {
                            BucketName = _bucketName,
                            Key = s3Object.Key
                        };
                        // Await the deletion of the individual object.
                        await _s3Client.DeleteObjectAsync(deleteRequest);
                    }
                }

                _logger.LogInformation("Successfully renamed folder from {oldPrefix} to {newPrefix}", oldPrefix, newPrefix);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error renaming folder from {oldPath} to {newPath}", oldFullPath, newFullPath);
            }
        }
        #endregion

    }   // end of class
//}   // end of namespace