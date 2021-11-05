using System;
using System.Collections.Generic;
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
    public partial class MainWindow : Window
    {
        private Timer _timer;
        private int _fileCount;
        private int _folderCount;
        private const int FileTarget = 299;
        private const int FolderTarget = 7;

        public MainWindow()
        {
            InitializeComponent();
            _timer = new Timer(Callback, null, Timeout.Infinite, Timeout.Infinite);
        }

        private void Callback(object? state)
        {
            if (!CheckForInstall())
            {
                return;
            }

            _folderCount = Directory.GetDirectories(@"C:\Program Files\Ultimaker Cura 4.10.0").Length;
            _fileCount = 0;
            if (_folderCount == FolderTarget)
            {
                foreach (var dir in Directory.GetDirectories(@"C:\Program Files\Ultimaker Cura 4.10.0"))
                {
                    _fileCount += Directory.GetFiles(dir).Length;
                }
                _fileCount += Directory.GetFiles(@"C:\Program Files\Ultimaker Cura 4.10.0").Length;
                _timer.Change(Timeout.Infinite, Timeout.Infinite);
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
            RunInstaller();
        }

        private bool CheckForInstall()
        {
            //TODO - look at program files for "C:\Program Files\Ultimaker Cura 4.10.0"
            var programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            return Directory.Exists(System.IO.Path.Combine(programFiles, @"Ultimaker Cura 4.10.0"));
        }

        private void DeleteExistingFiles()
        {
            //TODO - Delete the definitions, extruders, materials, variants folders  
            var programFiles = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),@"Ultimaker Cura 4.10.0");
            DeleteDirectory(Path.Combine(programFiles, "resources", "definitions"));
            DeleteDirectory(Path.Combine(programFiles, "resources", "extruders"));
            DeleteDirectory(Path.Combine(programFiles, "resources", "materials"));
            DeleteDirectory(Path.Combine(programFiles, "resources", "variants"));  
        }

        private void CopyNewFiles()
        {
            var filesDir = Path.Combine(System.AppContext.BaseDirectory, "CuraFiles");
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
            if (!CheckForInstall())
            {
                var filePath = Path.Combine(System.AppContext.BaseDirectory, "curainstaller.exe");
                if (File.Exists(filePath))
                {
                    Process.Start(filePath, "/S");
                }
            }
            _timer.Change(0, 250);
        }
    }
}