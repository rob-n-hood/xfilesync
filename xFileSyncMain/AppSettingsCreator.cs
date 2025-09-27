using System.IO;

namespace AppSettingsGenerator
{
    public class AppSettingsCreator
    {
        /// <summary>
        /// Creates an appsettings.json file with predefined content.
        /// </summary>
        /// <param name="filePath">The path where the file will be created, including the filename.</param>
        public static void CreateAppSettingsFile(string filePath)
        {

            // Get the full path to the current user's "My Documents" folder.
            string myDocumentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            // Escape the backslashes in the path to make it a valid JSON string.
            string jsonReadyWatchPath = myDocumentsPath.Replace("\\", "\\\\");

            // Use string interpolation ($) to insert the dynamic path into the JSON content.
            // Double curly braces {{ and }} are used to escape the braces in an interpolated string.
            string jsonContent = $@"{{
  ""SyncSettings"": {{
    ""WatchPath"": ""{jsonReadyWatchPath}"",
    ""BucketName"": ""foldersync""
  }},
  ""S3Provider"": {{
    ""AWS_SERVICE_URL"": ""http://192.168.2.217:9000"",
    ""AWS_ACCESS_KEY_ID"": ""sb40IhCh"",
    ""AWS_SECRET_ACCESS_KEY"": ""sfE3rcw"",
    ""ForcePathStyle"": true
  }}
}}";


            try
            {
                // Write the string content to the specified file.
                // This will create the file if it doesn't exist, or overwrite it if it does.
                File.WriteAllText(filePath, jsonContent);
                Console.WriteLine($"Successfully created and saved {filePath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while creating the file: {ex.Message}");
            }
        }

        /// <summary>
        /// Example of how to use the CreateAppSettingsFile method.
        /// </summary>
        //public static void Main()
        //{
            // Define the filename. This will create the file in the application's current directory.
         //   string settingsFilePath = "appsettings.json";

           // CreateAppSettingsFile(settingsFilePath);
       // }
    }
}