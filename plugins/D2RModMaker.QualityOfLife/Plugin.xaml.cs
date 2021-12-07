using D2RModMaker.Api;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Controls;

namespace D2RModMaker.QualityOfLife
{
    /// <summary>
    /// Interaction logic for Plugin.xaml
    /// </summary>
    [Export(typeof(IPlugin))]
    public partial class Plugin : UserControl, IPlugin
    {
        public string PluginName { get; set; } = "Quality of Life";
        public dynamic Settings { get; set; } = new ExpandoObject();
        public UserControl UI { get { return this; } }

        public string[] RequiredFiles { get
            {
                HashSet<string> files = new HashSet<string>();
                if(Settings.ExpandedStash)
                {
                    files.Add(@"data:data/global/excel/inventory.txt");
                }
                if(Settings.ExpandedMercenaryEquipment)
                {
                    files.Add(@"data:data/global/excel/ItemTypes.txt");
                    files.Add(@"data:data/global/ui/Layouts/HirelingInventoryPanelHD.json");
                }
                if(Settings.ExpandedInventory)
                {
                    files.Add(@"data:data/global/ui/Layouts/PlayerInventoryOriginalLayoutHD.json");
                    files.Add(@"data:data/global/ui/Layouts/PlayerInventoryExpansionLayoutHD.json");
                }
                if(Settings.ExpandedStash)
                {
                    files.Add(@"data:data/global/ui/Layouts/BankExpansionLayoutHD.json");
                    files.Add(@"data:data/global/ui/Layouts/BankOriginalLayoutHD.json");
                }
                if (Settings.ExpandedCube)
                {
                    files.Add(@"data:data/global/ui/Layouts/HoradricCubeLayoutHD.json");
                }
                if(Settings.StartWithCube)
                {
                    files.Add(@"data:data/global/excel/charstats.txt");
                }
                if(Settings.SkipIntroVideos)
                {
                    files.Add(@"data:data/hd/global/video/BlizzardLogos.webm");
                    files.Add(@"data:data/hd/global/video/d2intro.webm");
                    files.Add(@"data:data/hd/global/video/LogoAnim.webm");
                }
                return files.ToArray();
            }
        }

        public Plugin()
        {
            Settings.ExpandedStash = true;
            Settings.ExpandedInventory = true;
            Settings.ExpandedCube = true;
            Settings.ExpandedMercenaryEquipment = true;
            Settings.SkipIntroVideos = true;
            Settings.StartWithCube = true;
            Settings.ShowILvls = true;

            InitializeComponent();
        }

        public void Execute(ExecuteContext Context)
        {
            foreach (var p in Assembly.GetExecutingAssembly().GetManifestResourceNames())
            {
                Debug.WriteLine(p);
            }

            using (var json = ReadResourceFile(PluginResources.Data))
            {
                using (var streamReader = new StreamReader(json))
                {
                    var constants = JObject.Parse(streamReader.ReadToEnd());
                    if (Settings.ExpandedInventory)
                    {
                        ExpandInventory(constants, Context);
                    }
                }
            }
            if(Settings.SkipIntroVideos)
            {
                File.Create(Context.ModFiles[@"data:data/hd/global/video/BlizzardLogos.webm"]).Close();
                File.Create(Context.ModFiles[@"data:data/hd/global/video/d2intro.webm"]).Close();
                File.Create(Context.ModFiles[@"data:data/hd/global/video/LogoAnim.webm"]).Close();
            }
        }


        private void ExpandInventory(JObject constants, ExecuteContext Context)
        {
            //Update inventory.txt
            var txt = TXTFile.Read(Context.ModFiles[@"data:data/global/excel/inventory.txt"]);
            foreach (var playerClass in Constants.PlayerClasses)
            {
                var row = txt.GetByColumnAndValue("class", playerClass);
                var inventory = (JObject)constants["ExpandedInventory"]["Classic"]["inventory.txt"];
                foreach (var property in inventory.Properties())
                {
                    row[property.Name].Value = property.Value.ToString();
                }
            }
            foreach (var playerClass in Constants.PlayerClasses)
            {
                var row = txt.GetByColumnAndValue("class", playerClass + "2");
                var inventory = (JObject)constants["ExpandedInventory"]["Expansion"]["inventory.txt"];
                foreach (var property in inventory.Properties())
                {
                    row[property.Name].Value = property.Value.ToString();
                }
            }

            //Update layouts (only xpac)
            var layouts = (JObject)constants["ExpandedInventory"]["Classic"]["Layouts"];
            foreach (var layout in layouts)
            {
                var file = Context.ModFiles[@$"data:data/global/ui/Layouts/{ layout.Key }.json"];
                var root = StreamUtils.ReadJSONFile(file);
                ProcessJSONChanges((JObject)layout.Value, root);
                StreamUtils.WriteJSONFile(file, root);
            }

            layouts = (JObject)constants["ExpandedInventory"]["Expansion"]["Layouts"];
            foreach (var layout in layouts)
            {
                var file = Context.ModFiles[@$"data:data/global/ui/Layouts/{ layout.Key }.json"];
                var root = StreamUtils.ReadJSONFile(file);
                ProcessJSONChanges((JObject)layout.Value, root);
                StreamUtils.WriteJSONFile(file, root);
            }

            //Update sprites
            StreamUtils.WriteStreamToFile(
                ReadResourceFile(PluginResources.background_expanded2_lowend)
                , Path.Combine(Context.ModPath, @"data/hd/global/ui/panel/inventory/background_expanded2.lowend.sprite"));


            StreamUtils.WriteStreamToFile(
                ReadResourceFile(PluginResources.background_expanded2)
                , Path.Combine(Context.ModPath, @"data/hd/global/ui/panel/inventory/background_expanded2.sprite"));

            txt.Write();

        }

        private static void ProcessJSONChanges(JObject layout, JObject root)
        {
            if (layout.ContainsKey("Deletes"))
            {
                var deletes = (JArray)layout["Deletes"];
                foreach (var delete in deletes.Children<JValue>())
                {
                    var match = root.SelectToken(delete.Value<string>());
                    match.Remove();
                }
            }
            if (layout.ContainsKey("Merges"))
            {
                var merges = (JArray)layout["Merges"];
                foreach (var merge in merges.Children<JObject>())
                {
                    var match = root.SelectToken(merge["Path"].Value<string>());
                    if (match.Type == JTokenType.Object)
                    {
                        ((JObject)match).Merge(merge["Value"]);
                    }
                    else
                    {
                        ((JValue)match).Value = ((JValue)merge["Value"]).Value;
                    }
                }
            }
        }

        public static Stream ReadResourceFile(string resource)
        {
            return Assembly.GetExecutingAssembly().GetManifestResourceStream(resource);
        }
    }
}
