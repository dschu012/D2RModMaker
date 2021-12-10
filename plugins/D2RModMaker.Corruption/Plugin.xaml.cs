using D2RModMaker.Api;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace D2RModMaker.Corruption
{
    /// <summary>
    /// Interaction logic for Plugin.xaml
    /// </summary>
    [Export(typeof(IPlugin))]
    public partial class Plugin : UserControl, IPlugin
    {
        //constants used for txt/json files
        private static readonly string ItemName = "Scroll of Corruption";
        private static readonly string ItemCode = "666";
        private static readonly int StartingStatId = 361;
        private static readonly string ItemTypeName = "Scroll of Corruption";
        private static readonly string ItemTypeCode = "corb";


        public string PluginName { get; set; } = "Item Corruption";
        public bool Enabled { get; set; } = true;

        public dynamic Settings { get; set; } = new ExpandoObject();

        public string[] RequiredFiles { get; } = new string[]
        {
            @"data:data/hd/items/items.json",
            @"data:data/global/excel/ItemTypes.txt",
            @"data:data/global/excel/ItemStatCost.txt",
            @"data:data/global/excel/misc.txt",
            @"data:data/global/excel/Properties.txt",
            @"data:data/global/excel/cubemain.txt",
            @"data:data/global/excel/UniqueItems.txt",
        };

        public UserControl UI { get { return this; } }

        public Plugin()
        {
            using (var json = ReadResourceFile(PluginResources.Data))
            {
                using (var streamReader = new StreamReader(json))
                {
                    Settings.ItemTiers = JToken.Parse(streamReader.ReadToEnd());
                }
            }
            InitializeComponent();
        }

        public void Execute(ExecuteContext Context)
        {
            //creates corruption orb item type
            AddCorruptionMiscItemType(Context);

            //creates the hidden stat saying the item has been corrupted
            AddCorruptionStat(Context);
            AddCorruptionProperty(Context);
            //creates the item that will do the corrupting when cubed
            AddCorruptionMiscItem(Context);
            AddCorruptionUniqueItem(Context);
            //creates the cube recipes based on the corruptor stat roll
            AddCubeRecipes(Context);

            //add assets
            AddItemsJsonAsset(Context);
        }
        
        private void AddItemsJsonAsset(ExecuteContext Context)
        {
            var file = Context.ModFiles[@"data:data/hd/items/items.json"];
            JArray root = (JArray)StreamUtils.ReadJSONFile(file);

            root.Add(new JObject {
              new JProperty( $"{ItemCode}", new JObject { new JProperty ( "asset", "scroll/identify_scroll" ) } )
            });

            StreamUtils.WriteJSONFile(file, root);
        }

        private void AddCorruptionMiscItemType(ExecuteContext Context)
        {
            var txt = TXTFile.Read(Context.ModFiles[@"data:data/global/excel/ItemTypes.txt"]);
            var row = txt.GetByColumnAndValue("Code", $"{ItemTypeCode}");
            if (row == null)
            {
                row = new TXTRow(txt.Columns, new string[txt.Columns.Count]);
                txt.Rows.Add(row);
            }

            row["ItemType"].Value = $"{ItemTypeName}";
            row["Code"].Value = $"{ItemTypeCode}";
            row["Equiv1"].Value = "misc";
            row["Equiv2"].Value = "";
            row["Repair"].Value = "0";
            row["Body"].Value = "0";
            row["BodyLoc1"].Value = "";
            row["BodyLoc2"].Value = "";
            row["Shoots"].Value = "";
            row["Quiver"].Value = "";
            row["Throwable"].Value = "0";
            row["Reload"].Value = "0";
            row["ReEquip"].Value = "0";
            row["AutoStack"].Value = "0";
            row["Magic"].Value = "";
            row["Rare"].Value = "";
            row["Normal"].Value = "0";
            row["Beltable"].Value = "0";
            row["MaxSockets1"].Value = "0";
            row["MaxSocketsLevelThreshold1"].Value = "25";
            row["MaxSockets2"].Value = "0";
            row["MaxSocketsLevelThreshold2"].Value = "40";
            row["MaxSockets3"].Value = "0";
            row["TreasureClass"].Value = "0";
            row["Rarity"].Value = "0";
            row["StaffMods"].Value = "";
            row["Class"].Value = "";
            row["VarInvGfx"].Value = "0";
            row["InvGfx1"].Value = "";
            row["InvGfx2"].Value = "";
            row["InvGfx3"].Value = "";
            row["InvGfx4"].Value = "";
            row["InvGfx5"].Value = "";
            row["InvGfx6"].Value = "";
            row["StorePage"].Value = "misc";
            row["*eol"].Value = "0";

            txt.Write();
        }

        private void AddCorruptionStat(ExecuteContext Context)
        {
            var txt = TXTFile.Read(Context.ModFiles[@"data:data/global/excel/ItemStatCost.txt"]);
            var row = txt.GetByColumnAndValue("Stat", "item_corruption");
            if (row == null)
            {
                row = new TXTRow(txt.Columns, new string[txt.Columns.Count]);
                txt.Rows.Add(row);
            } 
            row["Stat"].Value = "item_corruption";
            row["*ID"].Value = StartingStatId.ToString("0");
            row["Send Other"].Value = "";
            row["Signed"].Value = "";
            row["Send Bits"].Value = "1";
            row["Send Param Bits"].Value = "";
            row["UpdateAnimRate"].Value = "";
            row["Saved"].Value = "";
            row["CSvSigned"].Value = "";
            row["CSvBits"].Value = "";
            row["CSvParam"].Value = "";
            row["fCallback"].Value = "";
            row["fMin"].Value = "";
            row["MinAccr"].Value = "";
            row["Encode"].Value = "";
            row["Add"].Value = "";
            row["Multiply"].Value = "";
            row["ValShift"].Value = "";
            row["1.09-Save Bits"].Value = "";
            row["1.09-Save Add"].Value = "";
            row["Save Bits"].Value = "2";
            row["Save Add"].Value = "0";
            row["Save Param Bits"].Value = "";
            row["keepzero"].Value = "";
            row["op"].Value = "";
            row["op param"].Value = "";
            row["op base"].Value = "";
            row["op stat1"].Value = "";
            row["op stat2"].Value = "";
            row["op stat3"].Value = "";
            row["direct"].Value = "";
            row["maxstat"].Value = "";
            row["damagerelated"].Value = "";
            row["itemevent1"].Value = "";
            row["itemeventfunc1"].Value = "";
            row["itemevent2"].Value = "";
            row["itemeventfunc2"].Value = "";
            row["descpriority"].Value = "254";
            row["descfunc"].Value = "3";
            row["descval"].Value = "1";
            row["descstrpos"].Value = "ModStr4m";
            row["descstrneg"].Value = "ModStr4m";
            row["descstr2"].Value = "";
            row["dgrp"].Value = "";
            row["dgrpfunc"].Value = "";
            row["dgrpval"].Value = "";
            row["dgrpstrpos"].Value = "";
            row["dgrpstrneg"].Value = "";
            row["dgrpstr2"].Value = "";
            row["stuff"].Value = "";
            row["advdisplay"].Value = "";
            row["*eol"].Value = "0";

            txt.Write();
        }

        private void AddCorruptionProperty(ExecuteContext Context)
        {
            var txt = TXTFile.Read(Context.ModFiles[@"data:data/global/excel/Properties.txt"]);
            var row = txt.GetByColumnAndValue("code", "corruption");
            if (row == null)
            {
                row = new TXTRow(txt.Columns, new string[txt.Columns.Count]);
                txt.Rows.Add(row);
            }
            row["code"].Value = "corruption";
            row["*Enabled"].Value = "1";
            row["func1"].Value = "1";
            row["stat1"].Value = "item_corruption";
            row["set1"].Value = "";
            row["val1"].Value = "";
            row["func2"].Value = "";
            row["stat2"].Value = "";
            row["set2"].Value = "";
            row["val2"].Value = "";
            row["func3"].Value = "";
            row["stat3"].Value = "";
            row["set3"].Value = "";
            row["val3"].Value = "";
            row["func4"].Value = "";
            row["stat4"].Value = "";
            row["set4"].Value = "";
            row["val4"].Value = "";
            row["func5"].Value = "";
            row["stat5"].Value = "";
            row["set5"].Value = "";
            row["val5"].Value = "";
            row["func6"].Value = "";
            row["stat6"].Value = "";
            row["set6"].Value = "";
            row["val6"].Value = "";
            row["func7"].Value = "";
            row["stat7"].Value = "";
            row["set7"].Value = "";
            row["val7"].Value = "";
            row["*Tooltip"].Value = "";
            row["*Parameter"].Value = "";
            row["*Min"].Value = "";
            row["*Max"].Value = "";
            row["*Notes"].Value = "";
            row["*eol"].Value = "0";

            txt.Write();
        }

        private void AddCorruptionMiscItem(ExecuteContext Context)
        {
            var txt = TXTFile.Read(Context.ModFiles[@"data:data/global/excel/misc.txt"]);
            var row = txt.GetByColumnAndValue("name", $"{ItemName}");

            if (row == null)
            {
                row = new TXTRow(txt.Columns, new string[txt.Columns.Count]);
                txt.Rows.Add(row);
            }
            
            //all rows in an unmodified txt
            row["name"].Value = $"{ItemName}";
            row["compactsave"].Value = "0";
            row["version"].Value = "100";
            row["level"].Value = "1";
            row["ShowLevel"].Value = "0";
            row["levelreq"].Value = "0";
            row["reqstr"].Value = "";
            row["reqdex"].Value = "";
            row["rarity"].Value = "0";
            row["spawnable"].Value = "0";
            row["speed"].Value = "0";
            row["nodurability"].Value = "1";
            row["cost"].Value = "0";
            row["gamble cost"].Value = "";
            row["code"].Value = $"{ItemCode}";
            row["alternategfx"].Value = "rsc";
            row["namestr"].Value = $"{ItemCode}";
            row["component"].Value = "16";
            row["invwidth"].Value = "1";
            row["invheight"].Value = "1";
            row["hasinv"].Value = "0";
            row["gemsockets"].Value = "0";
            row["gemapplytype"].Value = "0";
            row["flippyfile"].Value = "flprsc";
            row["invfile"].Value = "invrsc";
            row["uniqueinvfile"].Value = "invrsc";
            row["Transmogrify"].Value = "0";
            row["TMogType"].Value = "xxx";
            row["TMogMin"].Value = "";
            row["TMogMax"].Value = "";
            row["useable"].Value = "0";
            row["type"].Value = "corb";
            row["type2"].Value = "";
            row["dropsound"].Value = "item_scroll";
            row["dropsfxframe"].Value = "10";
            row["usesound"].Value = "item_scroll";
            row["unique"].Value = "1";
            row["transparent"].Value = "0";
            row["transtbl"].Value = "5";
            row["lightradius"].Value = "0";
            row["belt"].Value = "0";
            row["autobelt"].Value = "0";
            row["stackable"].Value = "0";
            row["minstack"].Value = "0";
            row["maxstack"].Value = "0";
            row["spawnstack"].Value = "0";
            row["quest"].Value = "";
            row["questdiffcheck"].Value = "";
            row["missiletype"].Value = "0";
            row["spellicon"].Value = "-1";
            row["pSpell"].Value = "";
            row["state"].Value = "";
            row["cstate1"].Value = "";
            row["cstate2"].Value = "";
            row["len"].Value = "";
            row["stat1"].Value = "";
            row["calc1"].Value = "";
            row["stat2"].Value = "";
            row["calc2"].Value = "";
            row["stat3"].Value = "";
            row["calc3"].Value = "";
            row["spelldesc"].Value = "";
            row["spelldescstr"].Value = "";
            row["spelldescstr2"].Value = "";
            row["spelldesccalc"].Value = "";
            row["spelldesccolor"].Value = "";
            row["durwarning"].Value = "0";
            row["qntwarning"].Value = "0";
            row["gemoffset"].Value = "0";
            row["BetterGem"].Value = "non";
            row["bitfield1"].Value = "0";
            row["CharsiMin"].Value = "";
            row["CharsiMax"].Value = "";
            row["CharsiMagicMin"].Value = "";
            row["CharsiMagicMax"].Value = "";
            row["CharsiMagicLvl"].Value = "255";
            row["GheedMin"].Value = "";
            row["GheedMax"].Value = "";
            row["GheedMagicMin"].Value = "";
            row["GheedMagicMax"].Value = "";
            row["GheedMagicLvl"].Value = "255";
            row["AkaraMin"].Value = "";
            row["AkaraMax"].Value = "";
            row["AkaraMagicMin"].Value = "";
            row["AkaraMagicMax"].Value = "";
            row["AkaraMagicLvl"].Value = "255";
            row["FaraMin"].Value = "";
            row["FaraMax"].Value = "";
            row["FaraMagicMin"].Value = "";
            row["FaraMagicMax"].Value = "";
            row["FaraMagicLvl"].Value = "255";
            row["LysanderMin"].Value = "";
            row["LysanderMax"].Value = "";
            row["LysanderMagicMin"].Value = "";
            row["LysanderMagicMax"].Value = "";
            row["LysanderMagicLvl"].Value = "255";
            row["DrognanMin"].Value = "";
            row["DrognanMax"].Value = "";
            row["DrognanMagicMin"].Value = "";
            row["DrognanMagicMax"].Value = "";
            row["DrognanMagicLvl"].Value = "255";
            row["HratliMin"].Value = "";
            row["HratliMax"].Value = "";
            row["HratliMagicMin"].Value = "";
            row["HratliMagicMax"].Value = "";
            row["HratliMagicLvl"].Value = "255";
            row["AlkorMin"].Value = "";
            row["AlkorMax"].Value = "";
            row["AlkorMagicMin"].Value = "";
            row["AlkorMagicMax"].Value = "";
            row["AlkorMagicLvl"].Value = "255";
            row["OrmusMin"].Value = "";
            row["OrmusMax"].Value = "";
            row["OrmusMagicMin"].Value = "";
            row["OrmusMagicMax"].Value = "";
            row["OrmusMagicLvl"].Value = "255";
            row["ElzixMin"].Value = "";
            row["ElzixMax"].Value = "";
            row["ElzixMagicMin"].Value = "";
            row["ElzixMagicMax"].Value = "";
            row["ElzixMagicLvl"].Value = "255";
            row["AshearaMin"].Value = "";
            row["AshearaMax"].Value = "";
            row["AshearaMagicMin"].Value = "";
            row["AshearaMagicMax"].Value = "";
            row["AshearaMagicLvl"].Value = "255";
            row["CainMin"].Value = "";
            row["CainMax"].Value = "";
            row["CainMagicMin"].Value = "";
            row["CainMagicMax"].Value = "";
            row["CainMagicLvl"].Value = "255";
            row["HalbuMin"].Value = "";
            row["HalbuMax"].Value = "";
            row["HalbuMagicMin"].Value = "";
            row["HalbuMagicMax"].Value = "";
            row["HalbuMagicLvl"].Value = "255";
            row["MalahMin"].Value = "";
            row["MalahMax"].Value = "";
            row["MalahMagicMin"].Value = "";
            row["MalahMagicMax"].Value = "";
            row["MalahMagicLvl"].Value = "255";
            row["LarzukMin"].Value = "";
            row["LarzukMax"].Value = "";
            row["LarzukMagicMin"].Value = "";
            row["LarzukMagicMax"].Value = "";
            row["LarzukMagicLvl"].Value = "255";
            row["AnyaMin"].Value = "";
            row["AnyaMax"].Value = "";
            row["AnyaMagicMin"].Value = "";
            row["AnyaMagicMax"].Value = "";
            row["AnyaMagicLvl"].Value = "255";
            row["JamellaMin"].Value = "";
            row["JamellaMax"].Value = "";
            row["JamellaMagicMin"].Value = "";
            row["JamellaMagicMax"].Value = "";
            row["JamellaMagicLvl"].Value = "255";
            row["Transform"].Value = "5";
            row["InvTrans"].Value = "8";
            row["SkipName"].Value = "1";
            row["NightmareUpgrade"].Value = "xxx";
            row["HellUpgrade"].Value = "xxx";
            row["mindam"].Value = "";
            row["maxdam"].Value = "";
            row["PermStoreItem"].Value = "";
            row["multibuy"].Value = "";
            row["Nameable"].Value = "";
            row["worldevent"].Value = "";

            txt.Write();
        }

        private void AddOrEditUniqueItem(TXTFile txt, Dictionary<string, string> values)
        {
            var row = new TXTRow(txt.Columns, new string[txt.Columns.Count]);
            txt.Rows.Add(row);

            foreach (var col in txt.Columns)
            {
                row[col.Value].Value = "";
            }

            row["version"].Value = "100";
            row["enabled"].Value = "1";
            row["*eol"].Value = "0"; ;

            foreach (var value in values)
            {
                row[value.Key].Value = value.Value;
            }
        }

        private void AddCorruptionUniqueItem(ExecuteContext Context)
        {
            var txt = TXTFile.Read(Context.ModFiles[@"data:data/global/excel/UniqueItems.txt"]);

            var settings = (IDictionary<string, object>)Settings;
            int max = 0;
            //how many unq items do we need to make

            var arr = (JArray)Settings.ItemTiers;
            foreach(var entry in arr)
            {
                int count = 0;
                count += entry["brick"].Value<int>();
                foreach(var tier in entry["tiers"])
                {
                    int weight = tier["weight"].Value<int>();
                    int mods = tier["mods"].Count();
                    count += weight * mods;
                }
                if(count > max)
                {
                    max = count;
                }
            }

            for (int i = 1; i <= max; i++) { 
                AddOrEditUniqueItem(txt, new Dictionary<string, string>() {
                    { "index", $"{ItemName} {i.ToString("0")}" },
                    {"*ID", ""  },
                    {"rarity", "1" },
                    {"nolimit","1" },
                    { "lvl", "1"},
                    { "lvl req", "1" },
                    { "code", $"{ItemCode}" },
                    { "*ItemName", $"{ItemName} {i.ToString("0")}" },
                    { "cost mult", "5" },
                    { "cost add", "5000" },
                    { "invtransform", "cred" },
                    {  "prop1", "mana" },
                    { "min1", i.ToString("0") },
                    { "max1", i.ToString("0") },
                });
            }

            txt.Write();
        }

        private void AddOrEditCubeMainRecipe(TXTFile txt, Dictionary<string,string> values)
        {
            var row = new TXTRow(txt.Columns, new string[txt.Columns.Count]);
            txt.Rows.Add(row);

            foreach (var col in txt.Columns)
            {
                row[col.Value].Value = "";
            }

            row["enabled"].Value = "1";
            row["version"].Value = "100";
            row["*eol"].Value = "0";

            foreach (var value in values)
            {
                row[value.Key].Value = value.Value;
            }
        }

        private string GetTierStats(JToken entry)
        {
            var brick = entry["brick"].Value<int>();
            var tierCount = entry["tiers"].Count();
            var tierCounts = new int[tierCount];
            var total = brick;
            for (int tierIdx = 0; tierIdx < tierCount; tierIdx++)
            {
                var tier = entry["tiers"][tierIdx];
                var weight = tier["weight"].Value<int>();
                tierCounts[tierIdx] = tier["mods"].Count() * weight;
                total += tierCounts[tierIdx];
            }
            var strings = new string[tierCount];
            for (int tierIdx = 0; tierIdx < tierCount; tierIdx++)
            {
                strings[tierIdx] = $"tier{tierIdx + 1}={(100.0 * (double)tierCounts[tierIdx] / (double)total).ToString("F")}";
            }
            return string.Join(",", strings) + $",brick={(100.0 * (double)brick / (double)total).ToString("F")}";
        }

        private void AddCubeRecipes(ExecuteContext Context)
        {
            var txt = TXTFile.Read(Context.ModFiles[@"data:data/global/excel/cubemain.txt"]);
            var row = new TXTRow(txt.Columns, new string[txt.Columns.Count]);
            txt.Rows.Add(row);

            //testing recipes
            AddOrEditCubeMainRecipe(txt, new Dictionary<string, string>() {
                { "description", "The Gnasher"},
                {"numinputs", "1" },
                {"input 1", "isc" },
                {"output","The Gnasher" },
                { "lvl", "99"}
            });

            AddOrEditCubeMainRecipe(txt, new Dictionary<string, string>() {
                { "description", $"{ItemName}"},
                {"numinputs", "1" },
                {"input 1", "hp1" },
                {"output",$"{ItemCode},uni" },
                //{ "lvl", "99"},
            });

            //real recipes
            AddOrEditCubeMainRecipe(txt, new Dictionary<string, string>() {
                { "description", $"Unique Item + Any Potion -> {ItemName}"},
                {"numinputs", "2" },
                {"input 1", "uni" },
                {"input 2", "poti" },
                {"output",$"{ItemCode},uni" },
                //{ "lvl", "99"},
            });
            AddOrEditCubeMainRecipe(txt, new Dictionary<string, string>() {
                { "description", $"Set Item + Any Potion -> {ItemName}"},
                {"numinputs", "2" },
                {"input 1", "set" },
                {"input 2", "poti" },
                {"output",$"{ItemCode},uni" },
                //{ "lvl", "99"},
            });

            //iterate data.json to build all recipes
            var arr = (JArray)Settings.ItemTiers;
            foreach(var entry in arr)
            {
                var type = entry["type"].Value<string>();
                var dictionary = new Dictionary<string, string>() {
                    //only allow if not already corrupted
                    { "op", "17" },
                    { "param", StartingStatId.ToString("0") },
                    { "value", "1" },

                    {"numinputs", "2" },
                    {"output","useitem" },
                    //{ "lvl", "99" },
                    {"mod 1","corruption" },
                    {"mod 1 min", "1" },
                    {"mod 1 max", "1" },
                };
                foreach(var q in entry["qualities"])
                {
                    int i = 1;
                    var quality = q.Value<string>();
                    var brick = entry["brick"].Value<int>();
                    var tierCount = entry["tiers"].Count();

                    AddOrEditCubeMainRecipe(txt, new Dictionary<string, string>() {
                        { "description", $"### Tier Stats ({type},{quality},{ GetTierStats(entry) }) ###"},
                        {"enabled", "0" }
                    });

                    for (int tierIdx = 0; tierIdx < tierCount; tierIdx++)
                    {
                        var tier = entry["tiers"][tierIdx];
                        var weight = tier["weight"].Value<int>();
                        int count = tier["mods"].Count() * weight;
                        for (int j = 0; j < count; j++)
                        {
                            dictionary["input 1"] = $"{type},{quality}";
                            dictionary["description"] = $"Corrupt {type},{quality},tier={tierIdx+1}";
                            dictionary["input 2"] = $"{ItemName} {(i++).ToString("0")}";
                            var mods = tier["mods"][j / weight];
                            for (int k = 0; k < 4; k++)
                            {
                                dictionary[$"mod {k + 2}"] = mods[$"stat{k + 1}"]?.Value<string>() ?? "";
                                dictionary[$"mod {k + 2} chance"] = mods[$"chance{k + 1}"]?.Value<string>() ?? "";
                                dictionary[$"mod {k + 2} param"] = mods[$"param{k + 1}"]?.Value<string>() ?? "";
                                dictionary[$"mod {k + 2} min"] = mods[$"min{k + 1}"]?.Value<string>() ?? "";
                                dictionary[$"mod {k + 2} max"] = mods[$"max{k + 1}"]?.Value<string>() ?? "";
                            }
                            AddOrEditCubeMainRecipe(txt, dictionary);
                        }
                    }
                    for (int k = 0; k < 4; k++)
                    {
                        dictionary[$"mod {k + 2}"] = "";
                        dictionary[$"mod {k + 2} chance"] = "";
                        dictionary[$"mod {k + 2} param"] = "";
                        dictionary[$"mod {k + 2} min"] = "";
                        dictionary[$"mod {k + 2} max"] = "";
                    }
                    for (int j = 0; j < brick; j++)
                    {
                        dictionary["input 1"] = $"{type},{quality}";
                        dictionary["description"] = $"Corrupt {type},{quality},brick";
                        dictionary["input 2"] = $"{ItemName} {(i++).ToString("0")}";
                        var output = Constants.BrickQualities[quality];
                        if(string.IsNullOrEmpty(output))
                        {
                            //eat the item
                            dictionary["output"] = $"";
                        } else
                        {
                            dictionary["output"] = $"usetype,{output}";
                        }
                        AddOrEditCubeMainRecipe(txt, dictionary);
                    }
                }
            }

            txt.Write();
        }

        public static Stream ReadResourceFile(string resource)
        {
            return Assembly.GetExecutingAssembly().GetManifestResourceStream(resource);
        }
    }
}
