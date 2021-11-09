using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
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


        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            _timer = new Timer(Callback, null, Timeout.Infinite, Timeout.Infinite);
            InstallButtonText = "Agree";
            StatusFontSize = 16;
            StatusText =
                $"Disclaimer by Gearbox3D LLC{Environment.NewLine}"
            + $"Please read this disclaimer carefully.{Environment.NewLine}{Environment.NewLine}"
            + $"Except when otherwise stated in writing, Gearbox3D LLC provides any Gearbox3D LLC software or third party software \"As is\" without warranty of any kind. The entire risk as to the quality and performance of Gearbox3D LLC software is with you.{Environment.NewLine}"
            + $"Unless required by applicable law or agreed to in writing, in no event will Gearbox3D LLC be liable to you for damages, including any general, special, incidental, or consequential damages arising out of the use or inability to use any Gearbox3D LLC software or third party software.";
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
            //Debug.WriteLine("Counting stuff");
            _folderCount = Directory.GetDirectories(@"C:\Program Files\Ultimaker Cura 4.10.0").Length;
            _fileCount = 0;
            if (_folderCount == FolderTarget)
            {
                //Debug.WriteLine("Folder count matches");
                foreach (var dir in Directory.GetDirectories(@"C:\Program Files\Ultimaker Cura 4.10.0"))
                {
                    _fileCount += Directory.GetFiles(dir).Length;
                }
                _fileCount += Directory.GetFiles(@"C:\Program Files\Ultimaker Cura 4.10.0").Length;
                if (_fileCount == FileTarget)
                {
                    StatusText += $"Done.{Environment.NewLine}";
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
                StatusText = "";
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
            //TODO - look at program files for "C:\Program Files\Ultimaker Cura 4.10.0"
            var programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            return Directory.Exists(Path.Combine(programFiles, @"Ultimaker Cura 4.10.0"));
        }

        private void DeleteExistingFiles()
        {
            StatusText += $"Deleting extra Cura definitions...{Environment.NewLine}";
            //TODO - Delete the definitions, extruders, materials, variants folders  
            var programFiles = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), @"Ultimaker Cura 4.10.0");
            DeleteDirectory(Path.Combine(programFiles, "resources", "definitions"));
            DeleteDirectory(Path.Combine(programFiles, "resources", "extruders"));
            DeleteDirectory(Path.Combine(programFiles, "resources", "materials"));
            DeleteDirectory(Path.Combine(programFiles, "resources", "variants"));
        }

        private void CopyNewFiles()
        {
            StatusText += $"Adding Gearbox3d Cura definitions...{Environment.NewLine}";
            var filesDir = Path.Combine(System.AppContext.BaseDirectory, "CuraFiles");
            var appdataDir = Path.Combine(System.AppContext.BaseDirectory, "Appdata");
            Console.WriteLine(filesDir);
            if (Directory.Exists(filesDir))
            {
                Console.WriteLine("Found dir");
                foreach (var dir in Directory.GetDirectories(filesDir))
                {
                    var folderName = Path.GetRelativePath(filesDir, dir);
                    DirectoryCopy(dir, Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), @"Ultimaker Cura 4.10.0", folderName), true);
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
            _timer.Change(0, 250);
            if (!CheckForInstall())
            {
                StatusText = "";
                StatusText += $"Installing Cura...{Environment.NewLine}";
                var filePath = Path.Combine(System.AppContext.BaseDirectory, "curainstaller.exe");
                if (File.Exists(filePath))
                {
                    Process.Start(filePath, "/S");
                }
                else
                {
                    StatusText = "Installer not found!!";
                }
            }
        }
    }
}