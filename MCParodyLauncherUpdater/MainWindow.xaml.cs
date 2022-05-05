﻿using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Media;
using System.Net;
using System.Windows;
using System.Windows.Input;

namespace MCParodyLauncherUpdater
{
    enum UpdaterStatus
    {
        checking,
        noUpdate,
        downloading,
        done
    }
    public partial class MainWindow : Window
    {
        private string rootPath;
        private string tempPath;
        private string mcplTempPath;
        private string versionFile;
        private string installerZip;
        private string installerDir;
        private string installer;

        private UpdaterStatus _status;

        internal UpdaterStatus Status
        {
            get => _status;
            set
            {
                _status = value;
                switch (_status)
                {
                    case UpdaterStatus.checking:
                        UpdaterStatusText.Text = "Checking for updates";
                        break;
                    case UpdaterStatus.noUpdate:
                        UpdaterStatusText.Text = "Checking for updates";
                        break;
                    case UpdaterStatus.downloading:
                        UpdaterStatusText.Text = "Downloading";
                        break;
                    case UpdaterStatus.done:
                        UpdaterStatusText.Text = "Download complete!";
                        break;
                }
            }

        }

        public MainWindow()
        {
            InitializeComponent();

            rootPath = Directory.GetCurrentDirectory();
            tempPath = Path.GetTempPath();
            mcplTempPath = Path.Combine(tempPath, "MCParodyLauncher");
            versionFile = Path.Combine(rootPath, "version.txt");
            installerZip = Path.Combine(tempPath, "MCParodyLauncher", "installer.zip");
            installerDir = Path.Combine(tempPath, "MCParodyLauncher", "installer");
            installer = Path.Combine(tempPath, "MCParodyLauncher", "installer", "MCParodyLauncherSetup.exe");
        }
        private void DelTemp()
        {
            if (Directory.Exists(installerDir))
            {
                Directory.Delete(installerDir, true);
            }
            if (Directory.Exists(mcplTempPath))
            {
                Directory.Delete(mcplTempPath, true);
            }
        }
        private void DelInstaller()
        {
            if (File.Exists(installer))
            {
                File.Delete(installer);
            }
        }
        private void CreateTemp()
        {
            Directory.CreateDirectory(mcplTempPath);
            Directory.CreateDirectory(installerDir);
        }
        private void CheckForUpdates()
        {
            Status = UpdaterStatus.checking;

            if (File.Exists(versionFile))
            {
                Version localVersion = new Version(File.ReadAllText(versionFile));
                VersionText.Text = localVersion.ToString();

                try
                {
                    WebClient webClient = new WebClient();
                    Version onlineVersion = new Version(webClient.DownloadString("https://raw.githubusercontent.com/KilLo445/mcpl-files/main/Launcher/version.txt"));

                    if (onlineVersion.IsDifferentThan(localVersion))
                    {
                        InstallUpdate(true, onlineVersion);
                    }
                    else
                    {
                        Status = UpdaterStatus.noUpdate;
                        SystemSounds.Exclamation.Play();
                        MessageBox.Show("No update is available.");
                        Close();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error checking for updates: {ex}");
                }
            }
            else
            {
                InstallUpdate(false, Version.zero);
            }
        }
        private void InstallUpdate(bool isUpdate, Version _onlineVersion)
        {
            Status = UpdaterStatus.downloading;

            DelInstaller();
            CreateTemp();

            WebClient webClient = new WebClient();

            webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(ProgressChanged);
            webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(DownloadCompletedCallback);

            webClient.DownloadFileAsync(new Uri("https://github.com/KilLo445/MCParodyLauncher/releases/download/main/MinecraftParodyLauncher.zip"), installerZip);
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            Status = UpdaterStatus.checking;
            CheckForUpdates();
        }
        private void ProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            DLProgress.Value = e.ProgressPercentage;
        }
        private void DownloadCompletedCallback(object sender, AsyncCompletedEventArgs e)
        {
            ZipFile.ExtractToDirectory(installerZip, installerDir);
            File.Delete(installerZip);
            Status = UpdaterStatus.done;
            Process.Start(installer);
            Close();
        }
        protected override void OnClosing(CancelEventArgs e)
        {
            if (Status == UpdaterStatus.downloading)
            {
                e.Cancel = true;
            }
            else
            {

            }
        }
        struct Version
        {
            internal static Version zero = new Version(0, 0, 0);

            private short major;
            private short minor;
            private short subMinor;

            internal Version(short _major, short _minor, short _subMinor)
            {
                major = _major;
                minor = _minor;
                subMinor = _subMinor;
            }
            internal Version(string _version)
            {
                string[] versionStrings = _version.Split('.');
                if (versionStrings.Length != 3)
                {
                    major = 0;
                    minor = 0;
                    subMinor = 0;
                    return;
                }

                major = short.Parse(versionStrings[0]);
                minor = short.Parse(versionStrings[1]);
                subMinor = short.Parse(versionStrings[2]);
            }

            internal bool IsDifferentThan(Version _otherVersion)
            {
                if (major != _otherVersion.major)
                {
                    return true;
                }
                else
                {
                    if (minor != _otherVersion.minor)
                    {
                        return true;
                    }
                    else
                    {
                        if (subMinor != _otherVersion.subMinor)
                        {
                            return true;
                        }
                    }
                }
                return false;
            }

            public override string ToString()
            {
                return $"{major}.{minor}.{subMinor}";
            }
        }

        private void Window_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }
    }
}
