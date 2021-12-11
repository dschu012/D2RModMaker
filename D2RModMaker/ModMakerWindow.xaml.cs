using D2RModMaker.Api;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using TACTLib.Client;
using TACTLib.Core.Product.D2;

namespace D2RModMaker
{
    /// <summary>
    /// Interaction logic for ModMaker.xaml
    /// </summary>
    public partial class ModMakerWindow : Window
    {
        private CompositionContainer _container;

        [ImportMany(typeof(IPlugin))]
        private IEnumerable<Lazy<IPlugin>> Plugins;

        private string _installPath;


        public ModMakerWindow()
        {
            InitializeComponent();
            if (!Directory.Exists("plugins"))
                Directory.CreateDirectory("plugins");

            _container = new CompositionContainer(new AggregateCatalog(
                new DirectoryCatalog(AppDomain.CurrentDomain.BaseDirectory),
                new DirectoryCatalog("plugins")
            ));
            _container.ComposeParts(this);

            foreach (var plugin in Plugins)
            {
                //this doesnt seem like a good idea
                var box = new GroupBox();
                box.Padding = new Thickness(5, 5, 5, 5);
                var header = new StackPanel { Orientation = Orientation.Horizontal };
                var checkbox = new CheckBox { VerticalAlignment = VerticalAlignment.Center, IsChecked = plugin.Value.Enabled };
                checkbox.Checked += (s, e) =>
                {
                    plugin.Value.Enabled = true;
                    box.Content = plugin.Value.UI;
                };
                checkbox.Unchecked += (s, e) =>
                {
                    plugin.Value.Enabled = false;
                    box.Content = null;
                };
                header.Children.Add(checkbox);
                header.Children.Add(new TextBlock { Text = plugin.Value.PluginName, Margin = new Thickness(5, 0, 0, 0) });
                box.Header = header;
                box.Content = plugin.Value.Enabled ? plugin.Value.UI : null;
                //((UserControl)box.Content).Visibility = plugin.Value.Enabled ? Visibility.Visible : Visibility.Hidden;
                PluginPanel.Children.Add(box);
            }
            _installPath = (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\Diablo II Resurrected", "InstallLocation", null);
            if (_installPath == null)
            {
                return;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var _clientHandler = new ClientHandler(_installPath, new ClientCreateArgs
            {
                Flavor = "retail"
            });
            var _productHandler = _clientHandler.ProductHandler as ProductHandler_D2R;
            /*
            foreach (var entry in CASCFile.Files)
            {
                Debug.WriteLine(entry.Value.FullName);
            }
            */
            ModInfo modInfo = new ModInfo();
            modInfo.Name = "d2rmm";
            modInfo.SavePath = modInfo.Name;
            modInfo.Path = System.IO.Path.Combine(_installPath, "mods", modInfo.Name, modInfo.Name + ".mpq");

            
            if(Directory.Exists(modInfo.Path))
            {
                Directory.Delete(modInfo.Path, true);
                //return;
            }
            
            Directory.CreateDirectory(modInfo.Path);


            modInfo.Files = Plugins
                .Where(plugin => plugin.Value.Enabled)
                .SelectMany(plugin => plugin.Value.RequiredFiles)
                .ToArray();

            ExecuteContext context = new ExecuteContext();
            context.ModPath = System.IO.Path.Combine(_installPath, "mods", modInfo.Name, modInfo.Name + ".mpq");
            context.UnmodifiedFiles = new Dictionary<string, string>();
            context.ModFiles = new Dictionary<string, string>();
            string tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

            
            foreach(var file in _productHandler.m_rootFiles)
            {
                if(file.FileName.Contains("hd/global/video"))
                {
                    Debug.WriteLine(file.FileName);
                }
                if (file.FileName.Contains("data/global/excel"))
                {
                    Debug.WriteLine(file.FileName);
                }
            }

            foreach (var file in modInfo.Files)
            {
                var casc = _productHandler.m_rootFiles.First(rootFile => rootFile.FileName == file);
                string fullPath = Path.Combine(modInfo.Path, file.Substring(5));
                using (Stream s = _clientHandler.OpenCKey(casc.MD5)) {
                    StreamUtils.WriteStreamToFile(s, fullPath);
                }
                context.ModFiles[file] = fullPath;
                string tempPath = Path.Combine(tempDir, file.Substring(5));
                using (Stream s = _clientHandler.OpenCKey(casc.MD5))
                {
                    StreamUtils.WriteStreamToFile(s, tempPath);
                }
                context.UnmodifiedFiles[file] = tempPath;
            }
            _clientHandler = null;
            _productHandler = null;
            //extracting the files uses a lot of memory, collect it here.
            GC.Collect();

            foreach (var plugin in Plugins)
            {
                if(plugin.Value.Enabled) { 
                    plugin.Value.Execute(context);
                }
            }

            File.WriteAllText(Path.Combine(modInfo.Path, "modinfo.json"), $"{{\n\t\"name\":\"{ modInfo.Name }\",\n\t\"savepath\":\"{ modInfo.SavePath }\"\n}}");

            Directory.Delete(tempDir, true);

            Debug.WriteLine("Done");
        }

        private void Button2_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(Path.Combine(_installPath, "D2R.exe"), "-mod d2rmm -txt");
        }

    }
}
