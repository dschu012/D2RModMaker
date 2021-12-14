using D2RModMaker.Api;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Composition;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using TACTLib.Client;
using TACTLib.Core.Product.D2;

namespace D2RModMaker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private string _installPath;

        public List<ModInfo> Mods { get; set; } = new List<ModInfo>();

        private ModMakerWindow _modMakerWindow;

        public MainWindow()
        {
            _installPath = (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\Diablo II Resurrected", "InstallLocation", null);
            if (_installPath == null)
            {
                MessageBox.Show("Could not find a valid D2R installation!", "Error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.None);
                Close();
            }
            RefreshMods();

            InitializeComponent();
            _modMakerWindow = new ModMakerWindow();
            _modMakerWindow.Show();
        }

        private void RefreshMods()
        {
            var modsDir = Path.Combine(_installPath, "mods");
            if (!Directory.Exists(modsDir))
            {
                Directory.CreateDirectory(modsDir);
            }
            foreach (var dir in Directory.GetDirectories(modsDir))
            {
                ModInfo modInfo = new ModInfo();
                modInfo.Name = Path.GetFileName(dir);
                Mods.Add(modInfo);
            }
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {

        }

        private void New_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Launch_Click(object sender, RoutedEventArgs e)
        {
            var mod = (ModInfo)ModsListView.SelectedItem;
            Process.Start(Path.Combine(_installPath, "D2R.exe"), $"-mod {mod.Name} -txt ${mod.Args}");
        }
    }
}
