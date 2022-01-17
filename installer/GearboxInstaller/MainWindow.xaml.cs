using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Iwsh = IWshRuntimeLibrary;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace GearboxInstaller
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private Timer _timer;
        private int _fileCount;
        private int _folderCount;
        private const int FileTarget = 299;
        private const int FolderTarget = 7;
        private string _statusText;
        private bool _agreementAccepted = false;
        private bool _installComplete;
        private System.Net.WebClient _webClient;
        private Uri _curaDownloadUrl = new(@"https://github.com/Ultimaker/Cura/releases/download/4.10.0/Ultimaker_Cura-4.10.0-amd64.exe");
        private string _installPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "GearboxSlicer");

        public event PropertyChangedEventHandler? PropertyChanged;

        public string StatusText
        {
            get { return _statusText; }
            set
            {
                _statusText = value;
                OnPropertyChanged();
            }
        }
        private string _installButtonText;

        public string InstallButtonText
        {
            get { return _installButtonText; }
            set
            {
                _installButtonText = value;
                OnPropertyChanged();
            }
        }
        private int _statusFontSize;

        public int StatusFontSize
        {
            get { return _statusFontSize; }
            set
            {
                _statusFontSize = value;
                OnPropertyChanged();
            }
        }
        private double _downloadProgress;

        public double DownloadProgress
        {
            get { return _downloadProgress; }
            set
            {
                _downloadProgress = value * 0.75;
                UpdateProgress();
            }
        }
        private double _installProgress;

        public double InstallProgress
        {
            get { return _installProgress * 100; }
            set
            {
                _installProgress = value * 0.25;
                UpdateProgress();
            }
        }
        private double _progress;

        public double Progress
        {
            get { return _progress; }
            set
            {
                if (_progress > value) { return; }
                _progress = value;
            }
        }
        private bool _installButtonEnabled;

        public bool InstallButtonEnabled
        {
            get { return _installButtonEnabled; }
            set
            {
                _installButtonEnabled = value;
                OnPropertyChanged();
            }
        }



        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            _timer = new Timer(Callback, null, Timeout.Infinite, Timeout.Infinite);
            _webClient = new();
            InstallButtonText = "Agree";
            StatusFontSize = 16;
            InstallButtonEnabled = true;
            _webClient.DownloadProgressChanged += (s, a) =>
            {
                DownloadProgress = a.ProgressPercentage;
                if (a.ProgressPercentage == 100)
                {
                    RunInstaller();
                }
            };
            StatusText =
                $"Disclaimer by Gearbox3D LLC{Environment.NewLine}"
            + $"Please read this disclaimer carefully.{Environment.NewLine}{Environment.NewLine}"
            + $"Except when otherwise stated in writing, Gearbox3D LLC provides any Gearbox3D LLC software or third party software \"As is\" without warranty of any kind. The entire risk as to the quality and performance of Gearbox3D LLC software is with you.{Environment.NewLine}"
            + $"Unless required by applicable law or agreed to in writing, in no event will Gearbox3D LLC be liable to you for damages, including any general, special, incidental, or consequential damages arising out of the use or inability to use any Gearbox3D LLC software or third party software.";
        }
        private void UpdateProgress()
        {
            Progress = ((double)(DownloadProgress) + InstallProgress);
        }
        private void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        private void Callback(object? state)
        {
            if (!CheckForInstall())
            {
                Debug.WriteLine("No install found");
                return;
            }
            _folderCount = Directory.GetDirectories(_installPath).Length;
            _fileCount = 0;
            InstallProgress = ((double)(_folderCount + _fileCount) / (FolderTarget + FileTarget));
            if (_folderCount == FolderTarget)
            {
                foreach (var dir in Directory.GetDirectories(_installPath))
                {
                    _fileCount += Directory.GetFiles(dir).Length;
                }
                _fileCount += Directory.GetFiles(_installPath).Length;
                InstallProgress = ((double)(_folderCount + _fileCount) / (FolderTarget + FileTarget));
                if (_fileCount == FileTarget)
                {
                    _timer.Change(Timeout.Infinite, Timeout.Infinite);
                    try
                    {
                        DeleteExistingFiles();
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e);
                    }

                    try
                    {
                        CopyNewFiles();
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e);
                    }
                }
            }
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (_installComplete)
            {
                Application.Current.Shutdown();
            }
            if (!_agreementAccepted)
            {
                _agreementAccepted = true;
                StatusText = "Click install to begin...";
                StatusFontSize = 20;
                InstallButtonText = "Install";
            }
            else
            {
                RunInstaller();
            }
        }

        private bool CheckForInstall()
        {
            //TODO - look at program files for "C:\Program Files\GearboxSlicer"
            return Directory.Exists(_installPath);
        }

        private void DeleteExistingFiles()
        {
            StatusText += $"Deleting extra Cura definitions...{Environment.NewLine}";
            //TODO - Delete the definitions, extruders, materials, variants folders
            DeleteDirectory(Path.Combine(_installPath, "resources", "definitions"));
            DeleteDirectory(Path.Combine(_installPath, "resources", "extruders"));
            DeleteDirectory(Path.Combine(_installPath, "resources", "materials"));
            DeleteDirectory(Path.Combine(_installPath, "resources", "variants"));
            DeleteDirectory(Path.Combine(_installPath, "plugins", "MonitorStage"));
            File.Delete(Path.Combine(@"C:\ProgramData\Microsoft\Windows\Start Menu\Programs\Ultimaker Cura", "Ultimaker Cura 4.10.0.lnk"));
            File.Delete(Path.Combine(@"C:\ProgramData\Microsoft\Windows\Start Menu\Programs\Ultimaker Cura", "Uninstall Ultimaker Cura 4.10.0.lnk")); 
            if (Directory.GetFiles(@"C:\ProgramData\Microsoft\Windows\Start Menu\Programs\Ultimaker Cura").All(x => !x.Contains("Ultimaker Cura")))
            {
                DeleteDirectory(@"C:\ProgramData\Microsoft\Windows\Start Menu\Programs\Ultimaker Cura");
            }
        }

        private void CopyNewFiles()
        {
            StatusText += $"Adding Gearbox3D Cura definitions...{Environment.NewLine}";
            var baseDir = Path.Combine(AppContext.BaseDirectory, "cura-files");
            var filesDir = Path.Combine(baseDir, "programfiles");
            var appdataDir = Path.Combine(baseDir, "appdata");
            Console.WriteLine(filesDir);
            if (Directory.Exists(filesDir))
            {
                Console.WriteLine("Found dir");
                foreach (var dir in Directory.GetDirectories(filesDir))
                {
                    var folderName = Path.GetRelativePath(filesDir, dir);
                    DirectoryCopy(dir, Path.Combine(_installPath, folderName), true);
                }
            }
            StatusText += $"Adding configuration...{Environment.NewLine}";
            if (Directory.Exists(appdataDir))
            {
                foreach (var dir in Directory.GetDirectories(appdataDir))
                {
                    if (Path.GetRelativePath(appdataDir, dir) == "cura")
                    {
                        DirectoryCopy(dir, Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "cura"), true);
                    }
                }
            }
            StatusText += "Done!";
            _installComplete = true;
            InstallButtonEnabled = true;
            CreateShortcut(Environment.GetFolderPath(Environment.SpecialFolder.Desktop));
            Directory.CreateDirectory(@"C:\ProgramData\Microsoft\Windows\Start Menu\Programs\GearboxSlicer");
            CreateShortcut(@"C:\ProgramData\Microsoft\Windows\Start Menu\Programs\GearboxSlicer");
            InstallButtonText = "Finish";
        }
        private void DeleteDirectory(string path)
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }
        }
        private void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            DirectoryInfo[] dirs = dir.GetDirectories();

            // If the destination directory doesn't exist, create it.       
            Directory.CreateDirectory(destDirName);

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string tempPath = Path.Combine(destDirName, file.Name);
                file.CopyTo(tempPath, true);
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string tempPath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, tempPath, copySubDirs);
                }
            }
        }
        private void RunInstaller()
        {
            InstallButtonEnabled = false;
            if (!CheckForInstall())
            {
                StatusText = "";
                _timer.Change(0, 250);
                StatusText += $"Installing Cura...{Environment.NewLine}";
                var filePath = Path.Combine(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName), "curainstaller.exe");
                var altFilePath = Path.Combine(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName),
                    "Ultimaker_Cura-4.10.0-amd64.exe");
                if (File.Exists(filePath))
                {
                    Process.Start(filePath, @$"/S /D={_installPath}");
                    StatusText = $"Running installer from {filePath}{Environment.NewLine}";
                }
                else if (File.Exists(altFilePath))
                {
                    Process.Start(altFilePath, @$"/S /D={_installPath}");
                    StatusText = $"Running installer from {altFilePath}{Environment.NewLine}";
                }   
                else
                {
                    _webClient.DownloadFileAsync(_curaDownloadUrl, Path.Combine(AppContext.BaseDirectory, "curainstaller.exe"));
                    StatusText = $"Downloading installer{Environment.NewLine}";
                }
            }
            else
            {
                StatusText = "Cura 4.10 install found, please uninstall before continuing";
                InstallButtonText = "Close";
                InstallButtonEnabled = true;
                _installComplete = true;
            }
        }
        private void CreateShortcut(string location)
        {
            string link = Path.Combine(location, "GearboxSlicer" + ".lnk");
            var shell = new Iwsh.WshShell();
            var shortcut = shell.CreateShortcut(link) as Iwsh.IWshShortcut;
            shortcut.TargetPath = Path.Combine(_installPath, "cura.exe");
            shortcut.WorkingDirectory = _installPath;
            shortcut.IconLocation = Path.Combine(_installPath, "resources", "images", "gbxslicer.ico");
            //shortcut...
            shortcut.Save();
        }
    }
}