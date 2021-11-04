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
        private Stopwatch _stopwatch = new();
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
            if (_folderCount == 7)
            {
                foreach (var dir in Directory.GetDirectories(@"C:\Program Files\Ultimaker Cura 4.10.0"))
                {
                    _fileCount += Directory.GetFiles(dir).Length;
                }
                _fileCount += Directory.GetFiles(@"C:\Program Files\Ultimaker Cura 4.10.0").Length;
                _timer.Change(Timeout.Infinite, Timeout.Infinite);
                DeleteExistingFiles();
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
            DeleteDirectory(Path.Combine(programFiles, "definitions"));
            DeleteDirectory(Path.Combine(programFiles, "extruders"));
            DeleteDirectory(Path.Combine(programFiles, "materials"));
            DeleteDirectory(Path.Combine(programFiles, "variants"));  
        }

        private void CopyNewFiles()
        {
            
        }

        private void DeleteDirectory(string path)
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }
        }

        private void RunInstaller()
        {
            var filePath = Path.Combine(System.AppContext.BaseDirectory, "curainstaller.exe");
            if (File.Exists(filePath))
            {
                Process.Start(filePath, "/S");
            }

            _stopwatch.Start();
            _timer.Change(0, 250);
        }
    }
}