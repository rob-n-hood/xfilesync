using AppSettingsGenerator;
using Microsoft.VisualBasic;
using SettingsEditor;
using System;
using System.Diagnostics;
using System.IO.Pipes;
using System.Text.Json;
using System.Windows.Forms;

namespace SyncTrayApp
{
    public partial class Form1 : Form
    {
        // A state variable to track if syncing is active
        private bool _isSyncing = true;
        private const string PipeName = "S3SyncServicePipe"; // The unique name for communication
                                                             // This path assumes the editor is in the same directory as the service's appsettings.json
                                                             //private readonly string _settingsPath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
                                                             //private readonly string _settingsPath = "C:\\Users\\Rob\\source\\repos\\xfilesync\\bin\\Debug\\net8.0\\appsettings.json";
        private string _settingsPath = "";
        private AppSettingsModel? _appSettings;
        private bool _isExiting = false; // Flag to allow the application to truly exit

        public Form1()
        {
            InitializeComponent();
            this.FormClosing += new FormClosingEventHandler(Form1_FormClosing);
            SettingsFile();
            LoadSettings();
            LaunchExe();
        }

        private void LaunchExe()
        {
            // looks to run the core program from the same directory as this tray app.
            string exePath = Path.Combine(AppContext.BaseDirectory, "xFileSyncHelper.exe");

            // Get the process name (e.g., "xfilesync") from the full path.
            string processName = Path.GetFileNameWithoutExtension(exePath);
            // Check if any process with the given name is already running.
            if (Process.GetProcessesByName(processName).Length > 0)
            {
                Console.WriteLine($"{processName}.exe is already running.");
                return; // Exit the method if the process is found.
            }


            if (File.Exists(exePath))
            {
                try
                {
                    ProcessStartInfo startInfo = new ProcessStartInfo
                    {
                        FileName = exePath,
                        UseShellExecute = false // Set to true if you want to use the OS shell to start the process
                    };
                    Process.Start(startInfo);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to launch xfilesync.exe: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show($"xfilesync.exe not found at path: {exePath}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }



        /// <summary>
        /// 
        /// </summary>
        private void SettingsFile()
        {

            // Construct the path to your app's folder in AppData
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string configPath = Path.Combine(appDataPath, "xFileSync");
            string destinationFilePath = Path.Combine(configPath, "appsettings.json");
            _settingsPath = destinationFilePath;

            if (!Directory.Exists(configPath))
            {
                Directory.CreateDirectory(configPath);
            }

            if (!File.Exists(_settingsPath))
            {
                Console.WriteLine($"Creating appsettings.json file at : {_settingsPath}");
                AppSettingsCreator.CreateAppSettingsFile(_settingsPath);
                int x = 0;
                while (x++ < 5 && !File.Exists(_settingsPath))
                {
                    Thread.Sleep(2000); // wait 2 seconds to ensure file is created before trying to read it.
                }
                if (!File.Exists(_settingsPath))
                {
                    Console.WriteLine($"Failed to create settings file at: {_settingsPath}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    //this.Close();
                    this.Visible = false;
                    return;
                }


            }
            else
            {
                Console.WriteLine($"appsettings.json file exists at : {_settingsPath}");
            }


            //string appName = "xfilesync";
            //string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            //string appSpecificFolderPath = Path.Combine(appDataPath, appName);
            //if (!Directory.Exists(appSpecificFolderPath))
            //{
            //    Directory.CreateDirectory(appSpecificFolderPath);
            //}

            //string destinationFilePath = Path.Combine(appSpecificFolderPath, "appsettings.json");
            //_settingsPath = destinationFilePath;

            //if (!File.Exists(destinationFilePath))
            //{
            //    string sourceFilePath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
            //    File.Copy(sourceFilePath, destinationFilePath, true); // true to overwrite if exists
            //    Console.WriteLine($"Created settings file at : {sourceFilePath}");
            //}
            //else
            //{
            //    Console.WriteLine($"Settings file already exists at : {destinationFilePath}");
            //}



        }

        /// <summary>
        /// Reads the appsettings.json file and populates the textboxes.
        /// </summary>
        private void LoadSettings()
        {
            try
            {
                if (!File.Exists(_settingsPath))
                {
                    MessageBox.Show($"Settings file not found at: {_settingsPath}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    //this.Close();
                    this.Visible = false;
                    return;
                }

                string jsonString = File.ReadAllText(_settingsPath);
                _appSettings = JsonSerializer.Deserialize<AppSettingsModel>(jsonString);

                // Populate the UI from the loaded settings
                txtWatchPath.Text = _appSettings?.SyncSettings?.WatchPath;
                txtBucketName.Text = _appSettings?.SyncSettings?.BucketName;
                txtServiceUrl.Text = _appSettings?.S3Provider?.AWS_SERVICE_URL;
                txtAccessKey.Text = _appSettings?.S3Provider?.AWS_ACCESS_KEY_ID;
                txtSecretKey.Text = _appSettings?.S3Provider?.AWS_SECRET_ACCESS_KEY;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading settings: {ex.Message}", "Load Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                //this.Close();
                this.Visible = false;
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            // Hide the main form on startup so only the tray icon is visible
            this.Visible = false;
            this.ShowInTaskbar = false;

            /////////////ServiceLauncher.EnsureServiceIsRunning();
        }

        private async void btnSave_Click_1(object sender, EventArgs e)
        {
            if (_appSettings == null) return;

            try
            {
                // Update the settings model from the UI textboxes
                _appSettings.SyncSettings.WatchPath = txtWatchPath.Text;
                _appSettings.SyncSettings.BucketName = txtBucketName.Text;
                _appSettings.S3Provider.AWS_SERVICE_URL = txtServiceUrl.Text;
                _appSettings.S3Provider.AWS_ACCESS_KEY_ID = txtAccessKey.Text;
                _appSettings.S3Provider.AWS_SECRET_ACCESS_KEY = txtSecretKey.Text;

                // Serialize the updated model back to a JSON string
                var options = new JsonSerializerOptions { WriteIndented = true };
                string updatedJson = JsonSerializer.Serialize(_appSettings, options);

                // Write the string back to the appsettings.json file
                File.WriteAllText(_settingsPath, updatedJson);

                // Send a command to the service to notify it of the change
                SendCommandToService("RELOAD_CONFIG");

                MessageBox.Show("Settings saved. The application will now restart.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                //this.Close();
                this.Visible = false;
                settingsToolStripMenuItem.Enabled = true;

                // Signal that a restart is required
                var result = DialogResult.OK;

                // Check if the user clicked "Save"
                if (result == DialogResult.OK)
                {
                    await RestartSyncServiceAsync();
                }

                this.Close();

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving settings: {ex.Message}", "Save Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task RestartSyncServiceAsync()
        {
            try
            {
                string exePath = Path.Combine(AppContext.BaseDirectory, "xfilesync.exe");
                string processName = Path.GetFileNameWithoutExtension(exePath);

                // Find the running process
                Process? syncProcess = Process.GetProcessesByName(processName).FirstOrDefault();

                if (syncProcess != null)
                {
                    //Log.LogInformation("Waiting for sync service to shut down...");
                    // Wait for up to 10 seconds for the process to exit
                    using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10)))
                    {
                        try
                        {
                            await syncProcess.WaitForExitAsync(cts.Token);
                        }
                        catch (OperationCanceledException)
                        {
                            // Handle timeout if needed
                        }
                    }


                    //_logger.LogInformation("Sync service has shut down.");
                }

                // Relaunch the executable
                //_logger.LogInformation("Relaunching the sync service.");
                LaunchExe();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while restarting the service: {ex.Message}", "Restart Error");
            }
        }



        private void btnCancel_Click_1(object sender, EventArgs e)
        {
            //this.Close();
            this.Visible = false;
            settingsToolStripMenuItem.Enabled = true;

        }


        private void sendAllFilesToCloudToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            notifyIcon1.ShowBalloonTip(1000, "Sync Started", "Starting a full sync of all files to the cloud.", ToolTipIcon.Info);

            // Send the command to the background service
            SendCommandToService("FORCE_SYNC_ALL");
        }

        private void stopSyncingToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            // Toggle the syncing state
            _isSyncing = !_isSyncing;

            if (_isSyncing)
            {

                // Update the menu text
                stopSyncingToolStripMenuItem.Text = "Pause Syncing";
                // Enable the "Send all files" option
                sendAllFilesToCloudToolStripMenuItem.Enabled = true;

                // Send a command to the service to RESUME syncing
                SendCommandToService("RESUME_SYNC");

            }
            else
            {
                // Update the menu text
                stopSyncingToolStripMenuItem.Text = "Start Syncing";
                // Disable the "Send all files" option as syncing is stopped
                sendAllFilesToCloudToolStripMenuItem.Enabled = false;

                // Send a command to the service to PAUSE syncing
                SendCommandToService("PAUSE_SYNC");
            }
        }


        private void closeExitSyncAppToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            // Set the flag to indicate we are intentionally exiting
            _isExiting = true;

            // Before closing, make sure the icon is not visible.
            notifyIcon1.Visible = false;

            // Send a command to stop the background service too.
            SendCommandToService("SHUTDOWN");

            // Exit the tray application
            Application.Exit();
        }


        /// <summary>
        /// Sends a command string to the background file watcher service via a named pipe.
        /// </summary>
        /// <param name="command">The command to send (e.g., "PAUSE_SYNC").</param>
        private void SendCommandToService(string command)
        {
            try
            {
                // Create a client stream to connect to the service's pipe server.
                using (var client = new NamedPipeClientStream(".", PipeName, PipeDirection.Out))
                {
                    // Connect with a 2-second timeout to avoid long waits if the service is not running.
                    client.Connect(2000);
                    using (var writer = new StreamWriter(client))
                    {
                        writer.WriteLine(command);
                        writer.Flush();
                    }
                }
            }
            catch (Exception ex)
            {
                // This will happen if the service is not running or can't be reached.
                MessageBox.Show($"Could not send command to sync service. It may not be running.\n\nError: {ex.Message}",
                                "Service Communication Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            settingsToolStripMenuItem.Enabled = false;
            this.Visible = true;
        }


        private void Form1_FormClosing(object? sender, FormClosingEventArgs e)
        {
            // Only cancel the close operation if we are NOT intentionally exiting.
            if (!_isExiting)
            {
                // Prevent the form from closing
                e.Cancel = true;
                // Hide the form instead of closing it
                this.Hide();
                settingsToolStripMenuItem.Enabled = true;   // enable the settings option again
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.Description = "Select a folder";
            folderBrowserDialog1.ShowNewFolderButton = true;
            folderBrowserDialog1.ShowNewFolderButton = true;
            folderBrowserDialog1.RootFolder = Environment.SpecialFolder.MyDocuments;

            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                string selectedPath = folderBrowserDialog1.SelectedPath;
                txtWatchPath.Text = selectedPath;
            }
        }
    }

}

