using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Media;
using System.Net;
using System.Windows;

namespace MCParodyLauncherUpdater
{
    enum UpdaterStatus
    {
        ready,
        downloading,
    }
    public partial class MainWindow : Window
    {
        private string rootPath;
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
                    case UpdaterStatus.ready:
                        UpdaterStatusText.Text = "Ready";
                        break;
                    case UpdaterStatus.downloading:
                        UpdaterStatusText.Text = "Downloading";
                        break;
                }
            }

        }

        public MainWindow()
        {
            InitializeComponent();

            rootPath = Directory.GetCurrentDirectory();
            versionFile = Path.Combine(rootPath, "version.txt");
            installerZip = Path.Combine(rootPath, "installer.zip");
            installerDir = Path.Combine(rootPath, "installer");
            installer = Path.Combine(rootPath, "installer", "MCParodyLauncherSetup.exe");

            Status = UpdaterStatus.ready;

            if (File.Exists(installer))
            {
                File.Delete(installer);
            }
            if (Directory.Exists(installerDir))
            {
                Directory.Delete(installerDir);
            }
        }
        private void CheckForUpdates()
        {
            if (File.Exists(versionFile))
            {
                Version localVersion = new Version(File.ReadAllText(versionFile));
                VersionText.Text = localVersion.ToString();

                try
                {
                    WebClient webClient = new WebClient();
                    Version onlineVersion = new Version(webClient.DownloadString("https://drive.google.com/uc?export=download&id=1rxN417kyFzZoGmRN1arAx9prpX2pAZPY"));

                    if (onlineVersion.IsDifferentThan(localVersion))
                    {
                        InstallUpdate(true, onlineVersion);
                    }
                    else
                    {
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

            WebClient webClient = new WebClient();

            webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(ProgressChanged);
            webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(DownloadCompletedCallback);

            webClient.DownloadFileAsync(new Uri("https://drive.google.com/uc?export=download&id=1HJiH0BT6pemGLwHAzG-52Cf2lEXB8jZ5"), installerZip);
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
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
            Process.Start(installer);
            Status = UpdaterStatus.ready;
            Close();
        }
        protected override void OnClosing(CancelEventArgs e)
        {
            if (Status == UpdaterStatus.downloading)
            {
                e.Cancel = true;
            }
            if (Status == UpdaterStatus.ready)
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
    }
}
