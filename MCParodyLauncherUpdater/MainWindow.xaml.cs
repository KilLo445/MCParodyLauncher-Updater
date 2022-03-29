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

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            Status = UpdaterStatus.downloading;

            WebClient webClient = new WebClient();

            webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(ProgressChanged);
            webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(DownloadCompletedCallback);
           
            webClient.DownloadFileAsync(new Uri("https://drive.google.com/uc?export=download&id=1HJiH0BT6pemGLwHAzG-52Cf2lEXB8jZ5"), installerZip);
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
    }
}
