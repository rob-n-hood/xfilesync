namespace SettingsEditor
{
    // These classes model the structure of your appsettings.json file
    // for easy serialization and deserialization.

    public class AppSettingsModel
    {
        public SyncSettings SyncSettings { get; set; } = new();
        public S3Provider S3Provider { get; set; } = new();
    }

    public class SyncSettings
    {
        public string? WatchPath { get; set; }
        public string? BucketName { get; set; }
    }

    public class S3Provider
    {
        public string? AWS_SERVICE_URL { get; set; }
        public string? AWS_ACCESS_KEY_ID { get; set; }
        public string? AWS_SECRET_ACCESS_KEY { get; set; }
        public bool? ForcePathStyle { get; set; }
    }
}