using D2RModMaker.Api;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Dynamic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace D2RModMaker.Difficulty
{
    /// <summary>
    /// Interaction logic for Plugin.xaml
    /// </summary>
    [Export(typeof(IPlugin))]
    public partial class Plugin : UserControl, IPlugin
    {

        public string PluginName { get; set; } = "Difficulty";
        public bool Enabled { get; set; } = true;
        public dynamic Settings { get; set; } = new ExpandoObject();
        public int DisplayOrder { get; } = int.MinValue;
        public int ExecutionOrder { get; } = int.MinValue;

        public string[] RequiredFiles { get; } = new string[]
        {
            @"data:data/global/excel/Levels.txt",
            @"data:data/global/excel/experience.txt",
            @"data:data/global/excel/monstats.txt",
            @"data:data/global/excel/MonLvl.txt"
        };

        public UserControl UI { get { return this; } }


        public Plugin()
        {
            Settings.RandomizeMonsters = false;
            Settings.MonsterQuanityScale = 1.0;
            Settings.HPScale = 1.0;
            Settings.DamageScale = 1.0;
            Settings.XPScale = 1.0;
            InitializeComponent();
        }

        public void Initialize(Window window) { }

        public void Execute(ExecuteContext Context)
        {
            if(Settings.RandomizeMonsters)
            {
                RandomizeMonsters(Context);
            }
            ScaleMonsters(Context);
        }

        private void ScaleMonsters(ExecuteContext Context)
        {
            var baselevelstxt = TXTFile.Read(Context.UnmodifiedFiles[@"data:data/global/excel/Levels.txt"]);
            var levelstxt = TXTFile.Read(Context.ModFiles[@"data:data/global/excel/Levels.txt"]);
            var basemonstattxt = TXTFile.Read(Context.UnmodifiedFiles[@"data:data/global/excel/monstats.txt"]);
            var monstattxt = TXTFile.Read(Context.ModFiles[@"data:data/global/excel/monstats.txt"]);

            foreach (var row in levelstxt.Rows)
            {
                var baserow = baselevelstxt.GetByColumnAndValue("Id", row["Id"].Value);
                if (baserow == null)
                {
                    continue;
                }

                row["MonDen"].Value = (baserow["MonDen"].ToUInt32() * Settings.MonsterQuanityScale).ToString("0");
                row["MonDen(N)"].Value = (baserow["MonDen(N)"].ToUInt32() * Settings.MonsterQuanityScale).ToString("0");
                row["MonDen(H)"].Value = (baserow["MonDen(H)"].ToUInt32() * Settings.MonsterQuanityScale).ToString("0");

                row["MonUMin"].Value = (baserow["MonUMin"].ToUInt32() * Settings.MonsterQuanityScale).ToString("0");
                row["MonUMax"].Value = (baserow["MonUMax"].ToUInt32() * Settings.MonsterQuanityScale).ToString("0");
                row["MonUMax(N)"].Value = (baserow["MonUMax(N)"].ToUInt32() * Settings.MonsterQuanityScale).ToString("0");
                row["MonUMax(H)"].Value = (baserow["MonUMax(H)"].ToUInt32() * Settings.MonsterQuanityScale).ToString("0");

                row["MonUMax(H)"].Value = (baserow["MonUMax(H)"].ToUInt32() * Settings.MonsterQuanityScale).ToString("0");
            }
            levelstxt.Write();


            foreach (var row in monstattxt.Rows)
            {
                var baserow = basemonstattxt.GetByColumnAndValue("Id", row["Id"].Value);
                if (baserow == null)
                {
                    continue;
                }
                row["minHP"].Value = (baserow["minHP"].ToUInt32() * Settings.HPScale).ToString("0");
                row["maxHP"].Value = (baserow["maxHP"].ToUInt32() * Settings.HPScale).ToString("0");
                row["MinHP(N)"].Value = (baserow["MinHP(N)"].ToUInt32() * Settings.HPScale).ToString("0");
                row["MaxHP(N)"].Value = (baserow["MaxHP(N)"].ToUInt32() * Settings.HPScale).ToString("0");
                row["MinHP(H)"].Value = (baserow["MinHP(H)"].ToUInt32() * Settings.HPScale).ToString("0");
                row["MaxHP(H)"].Value = (baserow["MaxHP(H)"].ToUInt32() * Settings.HPScale).ToString("0");

                row["A1MinD"].Value = (baserow["A1MinD"].ToUInt32() * Settings.DamageScale).ToString("0");
                row["A1MaxD"].Value = (baserow["A1MaxD"].ToUInt32() * Settings.DamageScale).ToString("0");
                row["S1MinD"].Value = (baserow["S1MinD"].ToUInt32() * Settings.DamageScale).ToString("0");
                row["S1MaxD"].Value = (baserow["S1MaxD"].ToUInt32() * Settings.DamageScale).ToString("0");
                row["A1MinD(N)"].Value = (baserow["A1MinD(N)"].ToUInt32() * Settings.DamageScale).ToString("0");
                row["A1MaxD(N)"].Value = (baserow["A1MaxD(N)"].ToUInt32() * Settings.DamageScale).ToString("0");
                row["S1MinD(N)"].Value = (baserow["S1MinD(N)"].ToUInt32() * Settings.DamageScale).ToString("0");
                row["S1MaxD(N)"].Value = (baserow["S1MaxD(N)"].ToUInt32() * Settings.DamageScale).ToString("0");
                row["A1MinD(H)"].Value = (baserow["A1MinD(H)"].ToUInt32() * Settings.DamageScale).ToString("0");
                row["A1MaxD(H)"].Value = (baserow["A1MaxD(H)"].ToUInt32() * Settings.DamageScale).ToString("0");
                row["S1MinD(H)"].Value = (baserow["S1MinD(H)"].ToUInt32() * Settings.DamageScale).ToString("0");
                row["S1MaxD(H)"].Value = (baserow["S1MaxD(H)"].ToUInt32() * Settings.DamageScale).ToString("0");


                row["Exp"].Value = (baserow["Exp"].ToUInt32() * Settings.XPScale).ToString("0");
                row["Exp(N)"].Value = (baserow["Exp(N)"].ToUInt32() * Settings.XPScale).ToString("0");
                row["Exp(H)"].Value = (baserow["Exp(H)"].ToUInt32() * Settings.XPScale).ToString("0");

            }
            monstattxt.Write();
        }

        private void RandomizeMonsters(ExecuteContext Context)
        {
            var levelstxt = TXTFile.Read(Context.ModFiles[@"data:data/global/excel/Levels.txt"]);
            var monstattxt = TXTFile.Read(Context.ModFiles[@"data:data/global/excel/monstats.txt"]);

            var replaceableMons = new HashSet<string>();
            foreach (var row in levelstxt.Rows)
            {
                for (int i = 1; i <= 25; i++)
                {
                    if (!string.IsNullOrWhiteSpace(row[$"mon{i}"]?.Value))
                    {
                        replaceableMons.Add(row[$"mon{i}"].Value);
                    }
                    if (!string.IsNullOrWhiteSpace(row[$"nmon{i}"]?.Value))
                    {
                        replaceableMons.Add(row[$"nmon{i}"].Value);
                    }
                    if (!string.IsNullOrWhiteSpace(row[$"umon{i}"]?.Value))
                    {
                        replaceableMons.Add(row[$"umon{i}"].Value);
                    }
                }
            }
            var randomized = new Queue<string>(replaceableMons
                .OrderBy(item => Context.Random.Next())
                .ToList());
            var swaps = new Dictionary<string, string>();
            while (randomized.Count > 0)
            {
                string first = randomized.Dequeue();
                if (randomized.Count == 0)
                {
                    swaps[first] = first;
                    continue;
                }
                string second = randomized.Dequeue();
                swaps[first] = second;
            }

            foreach (var row in levelstxt.Rows)
            {
                for (int i = 1; i <= 25; i++)
                {
                    if (!string.IsNullOrWhiteSpace(row[$"mon{i}"]?.Value))
                    {
                        row[$"mon{i}"].Value = GetKeyOrValue(swaps, row[$"mon{i}"].Value);
                    }
                    if (!string.IsNullOrWhiteSpace(row[$"nmon{i}"]?.Value))
                    {
                        row[$"nmon{i}"].Value = GetKeyOrValue(swaps, row[$"nmon{i}"].Value);
                    }
                    if (!string.IsNullOrWhiteSpace(row[$"umon{i}"]?.Value))
                    {
                        row[$"umon{i}"].Value = GetKeyOrValue(swaps, row[$"umon{i}"].Value);
                    }
                }
            }

            //swap the level and tc of the monsters. monlvl.txt will scale the difficulty (xp/dmg/hp/armor/ar) for us
            var cols = new string[]
            {
                "Level", "Level(N)", "Level(H)",
                "TreasureClass1", "TreasureClass2", "TreasureClass3", "TreasureClass4"
            };
            foreach (var entry in swaps)
            {
                var o1 = monstattxt.GetByColumnAndValue("Id", entry.Key);
                var o2 = monstattxt.GetByColumnAndValue("Id", entry.Value);
                foreach (var col in cols)
                {
                    var temp = o1[col].Value;
                    o1[col].Value = o2[col].Value;
                    o2[col].Value = temp;
                }
            }

            levelstxt.Write();
            monstattxt.Write();
        }
        private string GetKeyOrValue(Dictionary<string, string> swaps, string value)
        {
            if (swaps.ContainsKey(value))
            {
                return swaps[value];
            }
            if (swaps.ContainsValue(value))
            {
                return swaps.FirstOrDefault(x => x.Value == value).Key;
            }
            return value;
        }
    }
}
