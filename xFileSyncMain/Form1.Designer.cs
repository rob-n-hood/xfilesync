namespace SyncTrayApp
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            notifyIcon1 = new NotifyIcon(components);
            contextMenuStrip1 = new ContextMenuStrip(components);
            stopSyncingToolStripMenuItem = new ToolStripMenuItem();
            sendAllFilesToCloudToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator2 = new ToolStripSeparator();
            settingsToolStripMenuItem = new ToolStripMenuItem();
            closeExitSyncAppToolStripMenuItem = new ToolStripMenuItem();
            label1 = new Label();
            txtWatchPath = new TextBox();
            btnSave = new Button();
            btnCancel = new Button();
            label2 = new Label();
            txtBucketName = new TextBox();
            label3 = new Label();
            txtServiceUrl = new TextBox();
            label5 = new Label();
            txtAccessKey = new TextBox();
            label4 = new Label();
            txtSecretKey = new TextBox();
            toolTip1 = new ToolTip(components);
            folderBrowserDialog1 = new FolderBrowserDialog();
            btnBrowseFolders = new Button();
            contextMenuStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // notifyIcon1
            // 
            notifyIcon1.ContextMenuStrip = contextMenuStrip1;
            notifyIcon1.Icon = (Icon)resources.GetObject("notifyIcon1.Icon");
            notifyIcon1.Text = "xFile Sync Status";
            notifyIcon1.Visible = true;
            // 
            // contextMenuStrip1
            // 
            contextMenuStrip1.ImageScalingSize = new Size(32, 32);
            contextMenuStrip1.Items.AddRange(new ToolStripItem[] { stopSyncingToolStripMenuItem, sendAllFilesToCloudToolStripMenuItem, toolStripSeparator2, settingsToolStripMenuItem, closeExitSyncAppToolStripMenuItem });
            contextMenuStrip1.Name = "contextMenuStrip1";
            contextMenuStrip1.Size = new Size(326, 162);
            // 
            // stopSyncingToolStripMenuItem
            // 
            stopSyncingToolStripMenuItem.Name = "stopSyncingToolStripMenuItem";
            stopSyncingToolStripMenuItem.Size = new Size(325, 38);
            stopSyncingToolStripMenuItem.Text = "Pause Syncing";
            stopSyncingToolStripMenuItem.Click += stopSyncingToolStripMenuItem_Click_1;
            // 
            // sendAllFilesToCloudToolStripMenuItem
            // 
            sendAllFilesToCloudToolStripMenuItem.Name = "sendAllFilesToCloudToolStripMenuItem";
            sendAllFilesToCloudToolStripMenuItem.Size = new Size(325, 38);
            sendAllFilesToCloudToolStripMenuItem.Text = "Send All files to Cloud";
            sendAllFilesToCloudToolStripMenuItem.Click += sendAllFilesToCloudToolStripMenuItem_Click_1;
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new Size(322, 6);
            // 
            // settingsToolStripMenuItem
            // 
            settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
            settingsToolStripMenuItem.Size = new Size(325, 38);
            settingsToolStripMenuItem.Text = "Settings";
            settingsToolStripMenuItem.Click += settingsToolStripMenuItem_Click;
            // 
            // closeExitSyncAppToolStripMenuItem
            // 
            closeExitSyncAppToolStripMenuItem.Name = "closeExitSyncAppToolStripMenuItem";
            closeExitSyncAppToolStripMenuItem.Size = new Size(325, 38);
            closeExitSyncAppToolStripMenuItem.Text = "Close/Exit Sync App";
            closeExitSyncAppToolStripMenuItem.Click += closeExitSyncAppToolStripMenuItem_Click_1;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(43, 56);
            label1.Name = "label1";
            label1.Size = new Size(326, 32);
            label1.TabIndex = 1;
            label1.Text = "Local Folder to Sync to Cloud";
            // 
            // txtWatchPath
            // 
            txtWatchPath.Location = new Point(402, 56);
            txtWatchPath.Name = "txtWatchPath";
            txtWatchPath.Size = new Size(509, 39);
            txtWatchPath.TabIndex = 2;
            toolTip1.SetToolTip(txtWatchPath, "Defaults to MyDocuments");
            // 
            // btnSave
            // 
            btnSave.Location = new Point(179, 407);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(203, 46);
            btnSave.TabIndex = 11;
            btnSave.Text = "Save";
            btnSave.UseVisualStyleBackColor = true;
            btnSave.Click += btnSave_Click_1;
            // 
            // btnCancel
            // 
            btnCancel.Location = new Point(529, 407);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(203, 46);
            btnCancel.TabIndex = 12;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += btnCancel_Click_1;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(43, 220);
            label2.Name = "label2";
            label2.Size = new Size(157, 32);
            label2.TabIndex = 11;
            label2.Text = "Bucket Name";
            // 
            // txtBucketName
            // 
            txtBucketName.Location = new Point(402, 213);
            txtBucketName.Name = "txtBucketName";
            txtBucketName.Size = new Size(509, 39);
            txtBucketName.TabIndex = 12;
            toolTip1.SetToolTip(txtBucketName, "Name of Bucket defined on Relayer");
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(43, 140);
            label3.Name = "label3";
            label3.Size = new Size(139, 32);
            label3.TabIndex = 13;
            label3.Text = "Relayer URL";
            // 
            // txtServiceUrl
            // 
            txtServiceUrl.Location = new Point(402, 133);
            txtServiceUrl.Name = "txtServiceUrl";
            txtServiceUrl.Size = new Size(509, 39);
            txtServiceUrl.TabIndex = 14;
            toolTip1.SetToolTip(txtServiceUrl, "Ex. http://192.168.1.1:9000");
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(43, 272);
            label5.Name = "label5";
            label5.Size = new Size(130, 32);
            label5.TabIndex = 15;
            label5.Text = "Access Key";
            // 
            // txtAccessKey
            // 
            txtAccessKey.Location = new Point(402, 265);
            txtAccessKey.Name = "txtAccessKey";
            txtAccessKey.Size = new Size(509, 39);
            txtAccessKey.TabIndex = 16;
            toolTip1.SetToolTip(txtAccessKey, "Relayer Access Key");
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(43, 325);
            label4.Name = "label4";
            label4.Size = new Size(126, 32);
            label4.TabIndex = 17;
            label4.Text = "Secret Key";
            // 
            // txtSecretKey
            // 
            txtSecretKey.Location = new Point(402, 318);
            txtSecretKey.Name = "txtSecretKey";
            txtSecretKey.Size = new Size(509, 39);
            txtSecretKey.TabIndex = 18;
            toolTip1.SetToolTip(txtSecretKey, "Relayer Secret Key");
            // 
            // btnBrowseFolders
            // 
            btnBrowseFolders.Location = new Point(947, 52);
            btnBrowseFolders.Name = "btnBrowseFolders";
            btnBrowseFolders.Size = new Size(150, 46);
            btnBrowseFolders.TabIndex = 19;
            btnBrowseFolders.Text = "Browse";
            btnBrowseFolders.UseVisualStyleBackColor = true;
            btnBrowseFolders.Click += button1_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(13F, 32F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1155, 475);
            Controls.Add(btnBrowseFolders);
            Controls.Add(txtSecretKey);
            Controls.Add(label4);
            Controls.Add(txtAccessKey);
            Controls.Add(btnCancel);
            Controls.Add(label5);
            Controls.Add(btnSave);
            Controls.Add(txtServiceUrl);
            Controls.Add(txtWatchPath);
            Controls.Add(label3);
            Controls.Add(txtBucketName);
            Controls.Add(label1);
            Controls.Add(label2);
            Name = "Form1";
            Text = "xFile Settings";
            contextMenuStrip1.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private NotifyIcon notifyIcon1;
        private ContextMenuStrip contextMenuStrip1;
        private ToolStripMenuItem stopSyncingToolStripMenuItem;
        private ToolStripMenuItem closeExitSyncAppToolStripMenuItem;
        private ToolStripMenuItem sendAllFilesToCloudToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator2;
        private ToolStripMenuItem settingsToolStripMenuItem;
        private Label label1;
        private TextBox txtWatchPath;
        private Button btnSave;
        private Button btnCancel;
        private Label label2;
        private TextBox txtBucketName;
        private Label label3;
        private TextBox txtServiceUrl;
        private Label label5;
        private TextBox txtAccessKey;
        private Label label4;
        private TextBox txtSecretKey;
        private ToolTip toolTip1;
        private FolderBrowserDialog folderBrowserDialog1;
        private Button btnBrowseFolders;
    }
}
