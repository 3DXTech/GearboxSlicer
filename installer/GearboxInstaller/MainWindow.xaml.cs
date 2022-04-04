using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using GearboxInstaller.Enums;
using Iwsh = IWshRuntimeLibrary;

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
        private InstallerStates _installerState = InstallerStates.Unknown;
        private Uri _curaDownloadUrl =
            new(@"https://github.com/Ultimaker/Cura/releases/download/4.10.0/Ultimaker_Cura-4.10.0-amd64.exe");
        private string _installPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
            "GearboxSlicer");

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
        private string _primaryButtonText;

        public string PrimaryButtonText
        {
            get { return _primaryButtonText; }
            set
            {
                _primaryButtonText = value;
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
                if (_progress > value)
                {
                    return;
                }

                _progress = value;
            }
        }
        private bool _primaryButtonEnabled;
        private bool _altButtonDisplayed;
        private string _altButtonText;

        public bool PrimaryButtonEnabled
        {
            get { return _primaryButtonEnabled; }
            set
            {
                _primaryButtonEnabled = value;
                OnPropertyChanged();
            }
        }
        public bool AltButtonDisplayed
        {
            get => _altButtonDisplayed;
            set
            {
                _altButtonDisplayed = value;
                OnPropertyChanged();
            }
        }
        public string AltButtonText
        {
            get => _altButtonText;
            set
            {
                _altButtonText = value;
                OnPropertyChanged();
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            _timer = new Timer(Callback, null, Timeout.Infinite, Timeout.Infinite);
            _webClient = new();
            PrimaryButtonText = "Agree";
            StatusFontSize = 16;
            PrimaryButtonEnabled = true;
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
            
            if (_installerState is InstallerStates.Repair or InstallerStates.Uninstall or InstallerStates.Unknown)
            {
                if (_folderCount == 2)
                {
                    FullPurge();
                    if (_installerState is InstallerStates.Repair)
                    {
                        _timer.Change(Timeout.Infinite, Timeout.Infinite);
                        _installerState = InstallerStates.FreshInstall;
                        DownloadCura();
                    }
                    else if (_installerState is InstallerStates.Uninstall)
                    {
                        _timer.Change(Timeout.Infinite, Timeout.Infinite);
                        FullPurge();
                        StatusText = "Uninstall complete";
                        PrimaryButtonText = "Finish";
                        AltButtonDisplayed = false;
                        _installComplete = true;
                        PrimaryButtonEnabled = true;
                    }
                }

                return;
            }

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
        private void PrimaryButtonClicked(object sender, RoutedEventArgs e)
        {
            //"finish" button pressed
            if (_installComplete)
            {
                Application.Current.Shutdown();
            }

            //"agree" button pressed
            if (!_agreementAccepted)
            {
                _agreementAccepted = true;
                GetInstallerState();
                switch (_installerState)
                {
                    case InstallerStates.Update:
                        StatusText = "Cura install found, would you like to uninstall or update?";
                        StatusFontSize = 25;
                        AltButtonDisplayed = true;
                        AltButtonText = "Uninstall";
                        PrimaryButtonText = "Update";
                        break;
                    case InstallerStates.FreshInstall:
                        StatusText = "Click install to begin...";
                        StatusFontSize = 20;
                        PrimaryButtonText = "Install";
                        break;
                    case InstallerStates.Repair:
                        StatusText = "The Gearbox3d Slicer appears to have missing files, installation must be repaired.";
                        StatusFontSize = 20;
                        PrimaryButtonText = "Repair";
                        break;
                    case InstallerStates.Uninstall:
                        StatusText = "Uninstall";
                        break;
                    case InstallerStates.Unknown:
                        StatusText = "Unknown";
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            //install or update chosen (uninstall is alt button)
            else
            {
                var uninstallerPath = Path.Combine(_installPath, "Uninstall.exe");
                AltButtonDisplayed = false;
                switch (_installerState)
                {
                    case InstallerStates.Update:
                        //overwrite gearbox files, nothing else
                        UpdateFiles();
                        break;
                    case InstallerStates.FreshInstall:
                        //do the normal install
                        DownloadCura();
                        break;
                    case InstallerStates.Repair:
                        //remove all the current files and run a fresh install
                        //if uninstaller exists, run that first
                        if (File.Exists(uninstallerPath))
                        {
                            Process.Start(uninstallerPath, "/S");
                            _timer.Change(0, 250);
                        }
                        else
                        {
                            FullPurge();
                            DownloadCura();
                        }
                        break;
                    case InstallerStates.Uninstall:
                        //run Cura uninstaller and then purge GearboxSlicer folder
                        break;
                    case InstallerStates.Unknown:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
        private void GetInstallerState()
        {
            var uninstallerPath = Path.Combine(_installPath, "Uninstall.exe");
            if (!CheckForInstall())
            {
                _installerState = InstallerStates.FreshInstall;
            }
            else if (File.Exists(uninstallerPath))
            {
                _installerState = InstallerStates.Update;
            }
            else
            {
                _installerState = InstallerStates.Repair;
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
            var shortcutPath = Path.Combine(@"C:\ProgramData\Microsoft\Windows\Start Menu\Programs\Ultimaker Cura",
                "Ultimaker Cura 4.10.0.lnk");
            var uninstallShortcutPath = Path.Combine(
                @"C:\ProgramData\Microsoft\Windows\Start Menu\Programs\Ultimaker Cura",
                "Uninstall Ultimaker Cura 4.10.0.lnk");
            //Delete the definitions, extruders, materials, variants folders (to get rid of other company's stuff)
            DeleteDirectory(Path.Combine(_installPath, "resources", "definitions"));
            DeleteDirectory(Path.Combine(_installPath, "resources", "extruders"));
            DeleteDirectory(Path.Combine(_installPath, "resources", "materials"));
            DeleteDirectory(Path.Combine(_installPath, "resources", "variants"));
            DeleteDirectory(Path.Combine(_installPath, "resources", "quality"));
            DeleteDirectory(Path.Combine(_installPath, "plugins", "MonitorStage"));
            if (File.Exists(shortcutPath))
            {
                File.Delete(shortcutPath);
            }
            if (File.Exists(uninstallShortcutPath))
            {
                File.Delete(uninstallShortcutPath);
            }
            if (Directory.GetFiles(@"C:\ProgramData\Microsoft\Windows\Start Menu\Programs\Ultimaker Cura")
                .All(x => !x.Contains("Ultimaker Cura")))
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
                        DirectoryCopy(dir,
                            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "cura"),
                            true);
                    }
                }
            }

            StatusText += "Done!";
            _installComplete = true;
            PrimaryButtonEnabled = true;
            CreateShortcut(Environment.GetFolderPath(Environment.SpecialFolder.Desktop));
            Directory.CreateDirectory(@"C:\ProgramData\Microsoft\Windows\Start Menu\Programs\GearboxSlicer");
            CreateShortcut(@"C:\ProgramData\Microsoft\Windows\Start Menu\Programs\GearboxSlicer");
            PrimaryButtonText = "Finish";
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
        private void DownloadCura()
        {
            PrimaryButtonEnabled = false;
            StatusText = "";
            _timer.Change(0, 250);
            StatusText += $"Installing Cura...{Environment.NewLine}";
            var installArgs = @$"/S /D={_installPath}";
            var filePath = Path.Combine(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName),
                "curainstaller.exe");
            var altFilePath = Path.Combine(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName),
                "Ultimaker_Cura-4.10.0-amd64.exe");
            var downloadFilePath = Path.Combine(AppContext.BaseDirectory, "curainstaller.exe");
            if (File.Exists(filePath))
            {
                Process.Start(filePath, installArgs);
                StatusText = $"Running installer from {filePath}{Environment.NewLine}";
            }
            else if (File.Exists(altFilePath))
            {
                Process.Start(altFilePath, installArgs);
                StatusText = $"Running installer from {altFilePath}{Environment.NewLine}";
            }
            else if (File.Exists(downloadFilePath))
            {
                Process.Start(downloadFilePath, installArgs);
                StatusText = $"Running downloaded installer{Environment.NewLine}";
            }
            else
            {
                _webClient.DownloadFileAsync(_curaDownloadUrl,
                    Path.Combine(AppContext.BaseDirectory, "curainstaller.exe"));
                StatusText = $"Downloading installer{Environment.NewLine}";
            }
            
        }
        private void RunInstaller()
        {
            var installArgs = @$"/S /D={_installPath}";
            var filePath = Path.Combine(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName),
                "curainstaller.exe");
            var altFilePath = Path.Combine(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName),
                "Ultimaker_Cura-4.10.0-amd64.exe");
            var downloadFilePath = Path.Combine(AppContext.BaseDirectory, "curainstaller.exe");
            if (File.Exists(filePath))
            {
                Process.Start(filePath, installArgs);
                StatusText = $"Running installer from {filePath}{Environment.NewLine}";
            }
            else if (File.Exists(altFilePath))
            {
                Process.Start(altFilePath, installArgs);
                StatusText = $"Running installer from {altFilePath}{Environment.NewLine}";
            }
            else if (File.Exists(downloadFilePath))
            {
                Process.Start(downloadFilePath, installArgs);
                StatusText = $"Running downloaded installer{Environment.NewLine}";
            }
        }
        private void UpdateFiles()
        {
            DeleteExistingFiles();
            StatusText = $"Updating GearboxSlicer, overwriting resources files only{Environment.NewLine}";
            PrimaryButtonText = "Close";
            PrimaryButtonEnabled = false;
            CopyNewFiles();
            _installComplete = true;
            PrimaryButtonEnabled = true;
        }
        private void FullPurge()
        {
            if (Directory.Exists(_installPath))
            {
                try
                {
                    Directory.Delete(_installPath, true);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
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
            shortcut.Save();
        }
        private void AltButtonClicked(object sender, RoutedEventArgs e)
        {
            StatusText = "Uninstalling GearboxSlicer, please wait...";
            AltButtonDisplayed = false;
            PrimaryButtonEnabled = false;
            _installerState = InstallerStates.Uninstall;
            var uninstallerPath = Path.Combine(_installPath, "Uninstall.exe");
            Process.Start(uninstallerPath, "/S");
            _timer.Change(0, 250);
        }
    }
}