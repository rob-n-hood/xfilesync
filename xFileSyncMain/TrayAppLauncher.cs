using System;
using System.ServiceProcess;
using System.Windows.Forms;

namespace SyncTrayApp
{
    public class ServiceLauncher
    {
        // IMPORTANT: This name must exactly match the ServiceName you set
        // when you installed your file watcher service.
        private const string FileWatcherServiceName = "xFile Sync";

        /// <summary>
        // This method should be called when your tray application starts up,
        // for example, in the OnLoad event of your main form.
        /// </summary>
        public static void EnsureServiceIsRunning()
        {
            try
            {
                // Create a controller for your specific service
                ServiceController sc = new ServiceController(FileWatcherServiceName);

                // Check the status of the service
                if (sc.Status == ServiceControllerStatus.Stopped)
                {
                    Console.WriteLine($"Service '{FileWatcherServiceName}' is stopped. Attempting to start...");
                    // Start the service
                    sc.Start();
                    // Wait for the service to enter the 'Running' state (with a timeout)
                    sc.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(30));
                    Console.WriteLine($"Service '{FileWatcherServiceName}' has been started successfully.");
                }
                else
                {
                    Console.WriteLine($"Service '{FileWatcherServiceName}' is already running (Status: {sc.Status}).");
                }
            }
            catch (InvalidOperationException)
            {
                // This error occurs if the service is not installed on the computer.
                MessageBox.Show($"Error: The service '{FileWatcherServiceName}' is not installed.", "Service Not Found", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                // This can happen due to permissions issues (e.g., not running as admin)
                MessageBox.Show($"An error occurred while trying to start the service:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // --- How to integrate this into your Form1.cs ---
        /*
         
        // In your SyncTrayApp's Form1.cs:

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            this.Visible = false;
            this.ShowInTaskbar = false;

            // Call this method when the form loads
            ServiceLauncher.EnsureServiceIsRunning();
        }

        */
    }
}
