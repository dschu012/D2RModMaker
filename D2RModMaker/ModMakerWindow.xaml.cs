using D2RModMaker.Api;
using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
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
        private List<Lazy<IPlugin>> Plugins;

        private string _installPath;


        public ModMakerWindow()
        {
            InitializeComponent();

            _installPath = (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\Diablo II Resurrected", "InstallLocation", null);
            if (_installPath == null)
            {
                return;
            }

            //cant seem to get this working in a plugins folder...
            /*
            if (!Directory.Exists("plugins"))
                Directory.CreateDirectory("plugins");
            */
            _container = new CompositionContainer(new AggregateCatalog(
                new DirectoryCatalog(AppDomain.CurrentDomain.BaseDirectory)
                //new DirectoryCatalog(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "plugins"))
            ));
            _container.ComposeParts(this);


            Plugins.Sort((o1, o2) => o2.Value.DisplayOrder.CompareTo(o1.Value.DisplayOrder));
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

                plugin.Value.Initialize(this);

                PluginPanel.Children.Add(box);
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
            modInfo.Name = ModNameTextBox.Text;
            modInfo.SavePath = modInfo.Name;
            modInfo.Args = ModArgsTextBox.Text;
            var path = System.IO.Path.Combine(_installPath, "mods", modInfo.Name, modInfo.Name + ".mpq");

            
            if(Directory.Exists(path))
            {
                Directory.Delete(path, true);
                //return;
            }
            
            Directory.CreateDirectory(path);


            var files = Plugins
                .Where(plugin => plugin.Value.Enabled)
                .SelectMany(plugin => plugin.Value.RequiredFiles)
                .ToList();
            files.Add(Constants.Files.NEXT_STRING_ID);

            ExecuteContext context = new ExecuteContext();
            context.ModPath = System.IO.Path.Combine(_installPath, "mods", modInfo.Name, modInfo.Name + ".mpq");
            context.UnmodifiedFiles = new Dictionary<string, string>();
            context.ModFiles = new Dictionary<string, string>();
            string tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

            //debugging
#if DEBUG
            foreach (var file in _productHandler.m_rootFiles)
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
#endif

            foreach (var file in files)
            {
                var casc = _productHandler.m_rootFiles.First(rootFile => rootFile.FileName == file);
                string fullPath = Path.Combine(path, file.Substring(5));
                using (Stream s = _clientHandler.OpenCKey(casc.MD5)) {
                    Utils.WriteStreamToFile(s, fullPath);
                }
                context.ModFiles[file] = fullPath;
                string tempPath = Path.Combine(tempDir, file.Substring(5));
                using (Stream s = _clientHandler.OpenCKey(casc.MD5))
                {
                    Utils.WriteStreamToFile(s, tempPath);
                }
                context.UnmodifiedFiles[file] = tempPath;
            }
            _clientHandler = null;
            _productHandler = null;
            //extracting the files uses a lot of memory, collect it here.
            GC.Collect();

            Plugins.Sort((o1, o2) => o2.Value.ExecutionOrder.CompareTo(o1.Value.ExecutionOrder));

            Utils.Init(context);
            foreach (var plugin in Plugins)
            {
                if(plugin.Value.Enabled) {
                    modInfo.Plugins[plugin.Value.PluginName] = plugin.Value.Settings;
                    plugin.Value.Execute(context);
                }
            }
            Utils.Cleanup(context);

            Utils.WriteJSONFile(Path.Combine(path, "modinfo.json"), JObject.FromObject(modInfo));

            Directory.Delete(tempDir, true);

            Debug.WriteLine("Done");
        }

        private void Button2_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(Path.Combine(_installPath, "D2R.exe"), $"-mod {ModNameTextBox.Text} -txt {ModArgsTextBox.Text}");
        }

    }
}
