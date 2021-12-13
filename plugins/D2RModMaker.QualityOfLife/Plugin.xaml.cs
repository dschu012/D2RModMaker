using D2RModMaker.Api;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
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
        public bool Enabled { get; set; } = true;
        public dynamic Settings { get; set; } = new ExpandoObject();
        public int DisplayOrder { get; } = int.MaxValue;
        public int ExecutionOrder { get; } = int.MinValue;
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
                if(Settings.UseSkillsInTown)
                {
                    files.Add(@"data:data/global/excel/skills.txt");
                    files.Add(@"data:data/global/excel/Missiles.txt");
                }
                if(Settings.ShowILvls)
                {
                    files.Add(@"data:data/global/excel/armor.txt");
                    files.Add(@"data:data/global/excel/weapons.txt");
                    files.Add(@"data:data/global/excel/misc.txt");
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
            Settings.UseSkillsInTown = true;

            Settings.TPBookSize = 20.0;
            Settings.IDBookSize = 20.0;
            Settings.KeySize = 12.0;
            Settings.QuiverSize = 350.0;


            InitializeComponent();
        }

        public void Initialize(Window window) { }

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
            if(Settings.StartWithCube)
            {
                StartWithCube(Context);
            }
            if(Settings.SkipIntroVideos)
            {
                SkipIntroVideos(Context);
            }
            if(Settings.UseSkillsInTown)
            {
                UseSkillsInTown(Context);
            }
            if(Settings.ShowILvls)
            {
                ShowILvls(Context);
            }
            IncreaseStackSizes(Context);
        }

        private void StartWithCube(ExecuteContext Context)
        {
            var txt = TXTFile.Read(Context.ModFiles[@"data:data/global/excel/charstats.txt"]);
            foreach(var row in txt.Rows)
            {
                for(int i = 1; i <= 6; i++)
                {
                    var col = txt.Columns["item" + i];
                    //already have box as starting item
                    if(row[col].Value == "box")
                    {
                        break;
                    }
                    if((row[col].Value == "" || row[col].Value == "0")
                        && (row[col+2].Value == "" || row[col+2].Value == "0"))
                    {
                        row[col].Value = "box";
                        row[col + 2].Value = "1";
                        break;
                    }
                }
            }
            txt.Write();
        }

        private void IncreaseStackSizes(ExecuteContext Context)
        {
            var txt = TXTFile.Read(Context.ModFiles[@"data:data/global/excel/misc.txt"]);
            txt.GetByColumnAndValue("code", "tbk")["maxstack"].Value = GetStackSize(Settings.TPBookSize).ToString("0");
            txt.GetByColumnAndValue("code", "ibk")["maxstack"].Value = GetStackSize(Settings.IDBookSize).ToString("0");
            txt.GetByColumnAndValue("code", "key")["maxstack"].Value = GetStackSize(Settings.KeySize).ToString("0");
            txt.GetByColumnAndValue("code", "aqv")["maxstack"].Value = GetStackSize(Settings.QuiverSize).ToString("0");
            txt.GetByColumnAndValue("code", "cqv")["maxstack"].Value = GetStackSize(Settings.QuiverSize).ToString("0");
            txt.Write();
        }

        private double GetStackSize(double number)
        {
            if(number < 0.0)
            {
                return 0.0;
            }
            if(number > 511.0)
            {
                return 511.0;
            }
            return number;
        }

        private void ShowILvls(ExecuteContext Context)
        {
            var txt = TXTFile.Read(Context.ModFiles[@"data:data/global/excel/armor.txt"]);
            foreach (var row in txt.Rows)
            {
                row["ShowLevel"].Value = "1";
            }
            txt.Write();
            txt = TXTFile.Read(Context.ModFiles[@"data:data/global/excel/weapons.txt"]);
            foreach (var row in txt.Rows)
            {
                row["ShowLevel"].Value = "1";
            }
            txt.Write();
            txt = TXTFile.Read(Context.ModFiles[@"data:data/global/excel/misc.txt"]);
            foreach (var row in txt.Rows)
            {
                row["ShowLevel"].Value = "1";
            }
            txt.Write();
        }

        private void UseSkillsInTown(ExecuteContext Context)
        {
            var txt = TXTFile.Read(Context.ModFiles[@"data:data/global/excel/skills.txt"]);
            foreach(var row in txt.Rows)
            {
                row["InTown"].Value = "1";
            }
            txt.Write();
            txt = TXTFile.Read(Context.ModFiles[@"data:data/global/excel/Missiles.txt"]);
            foreach (var row in txt.Rows)
            {
                row["Town"].Value = "1";
            }
            txt.Write();
        }

        private void SkipIntroVideos(ExecuteContext Context)
        {
            File.Create(Context.ModFiles[@"data:data/hd/global/video/BlizzardLogos.webm"]).Close();
            File.Create(Context.ModFiles[@"data:data/hd/global/video/d2intro.webm"]).Close();
            File.Create(Context.ModFiles[@"data:data/hd/global/video/LogoAnim.webm"]).Close();
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
                var root = (JObject)StreamUtils.ReadJSONFile(file);
                ProcessJSONChanges((JObject)layout.Value, root);
                StreamUtils.WriteJSONFile(file, root);
            }

            layouts = (JObject)constants["ExpandedInventory"]["Expansion"]["Layouts"];
            foreach (var layout in layouts)
            {
                var file = Context.ModFiles[@$"data:data/global/ui/Layouts/{ layout.Key }.json"];
                var root = (JObject)StreamUtils.ReadJSONFile(file);
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
