using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Microsoft.VisualBasic;
using Serilog;
using System.Runtime.CompilerServices;
//using xfilesync;

// Construct the path to your app's folder in AppData for logging
string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
string logPath = Path.Combine(appDataPath, "xFileSync", "logs", "sync-log-.txt");

// Configure the static logger
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .WriteTo.File(logPath, rollingInterval: RollingInterval.Day) // Creates a new log file each day
    .CreateLogger();

Log.Information("Starting up the xFile Sync Core Program");

CheckForSettingsFile();

try
{ 
    IHost host = Host.CreateDefaultBuilder(args)   // defaults to appsettings.json
        .UseWindowsService(options =>
        {
            options.ServiceName = "xFile Sync";
        })
        //.UseContentRoot(AppContext.BaseDirectory)
        .ConfigureAppConfiguration((hostingContext, config) =>
        {
            // Construct the path to your app's folder in AppData
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string configPath = Path.Combine(appDataPath, "XFileSync");
            Directory.CreateDirectory(configPath);

            // Tell the host to look for appsettings.json in that specific folder
            config.SetBasePath(configPath);
            config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
        })
        .ConfigureServices((hostContext, services) =>
        {
            Log.Information($"Registering with the Relayer as an S3 client.");
            // Manually configure and register the IAmazonS3 client
            services.AddSingleton<IAmazonS3>(sp =>
            {
                var configSection = hostContext.Configuration.GetSection("S3Provider");
                var serviceUrl = configSection.GetValue<string>("AWS_SERVICE_URL");
                var accessKey = configSection.GetValue<string>("AWS_ACCESS_KEY_ID");
                var secretKey = configSection.GetValue<string>("AWS_SECRET_ACCESS_KEY");
                var forcePathStyle = configSection.GetValue<bool>("ForcePathStyle");

                if (string.IsNullOrEmpty(serviceUrl) || string.IsNullOrEmpty(accessKey) || string.IsNullOrEmpty(secretKey))
                {
                    throw new InvalidOperationException("S3Provider configuration is missing or incomplete in appsettings.json.");
                }

                // Disable checksums for uploads
                AWSConfigsS3.DisableDefaultChecksumValidation = true;

                    var config = new AmazonS3Config
                {
                    ServiceURL = serviceUrl,
                    ForcePathStyle = forcePathStyle,
                    // disable checksums for deletes
                    // RequestChecksumCalculation = Amazon.Runtime.RequestChecksumCalculation.WHEN_REQUIRED
                };

                var credentials = new BasicAWSCredentials(accessKey, secretKey);
                return new AmazonS3Client(credentials, config);
            });

            services.AddHostedService<Worker>();
        })
        .Build();

    await host.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application start-up failed");
}
finally
{
    Log.CloseAndFlush();
}

void CheckForSettingsFile([CallerFilePath] string callerFilePath = "")
{
    try
    {
        // Construct the path to your app's folder in AppData
        string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        string configPath = Path.Combine(appDataPath, "XFileSync");
        Directory.CreateDirectory(configPath);
        string settingsFilePath = Path.Combine(configPath, "appsettings.json");
        if (!File.Exists(settingsFilePath))
        {
           MsgBoxResult result = Interaction.MsgBox("The settings file was not found.", MsgBoxStyle.OkOnly, "Settings File Not Found");
        }
        else
        {
            Log.Information($"Settings file found at {settingsFilePath}");
        }
    }   
    catch (Exception ex)
    {
        Log.Error(ex, $"Error in CheckForSettingsFile called from {callerFilePath}");
        throw; // Re-throw the exception after logging it
    }
}