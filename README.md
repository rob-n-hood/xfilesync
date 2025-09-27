This is a Windows Application that is designed as a one way syncronization between a local folder and the ScPrime Storage Cloud.  
The applications runs as a background service and is accessible via a system tray icon.
xFileSync communicates with a user's ScPrime Relayer using the Amazon S3 protocol.

Installation Note: After installing using the .msi file, the application will need to be launched manually (or a reboot will automatically allow it to load from the user's Satrtup programs).

Security Note: The secretkey used to access the relayer is not encrypted and is stored in plain text in the applications configuration file (appsettings.json).
