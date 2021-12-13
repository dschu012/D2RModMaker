using D2RModMaker.Api;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Composition;
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

        private ModMakerWindow _modMakerWindow;

        public MainWindow()
        {
            _installPath = (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\Diablo II Resurrected", "InstallLocation", null);
            if (_installPath == null)
            {
                return;
            }

            InitializeComponent();
            _modMakerWindow = new ModMakerWindow();
            _modMakerWindow.Show();
        }

    }
}
