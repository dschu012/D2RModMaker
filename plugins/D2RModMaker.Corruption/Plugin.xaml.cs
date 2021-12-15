using D2RModMaker.Api;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace D2RModMaker.Corruption
{
    /// <summary>
    /// Interaction logic for Plugin.xaml
    /// </summary>
    [Export(typeof(IPlugin))]
    public partial class Plugin : UserControl, IPlugin
    {

        public string PluginName { get; set; } = "Item Corruption";
        public bool Enabled { get; set; } = true;
        public int DisplayOrder { get; } = int.MinValue;
        public int ExecutionOrder { get; } = int.MaxValue;

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

            @"data:data/local/lng/strings/item-modifiers.json",
            @"data:data/local/lng/strings/item-names.json",

            //@"data:data/global/ui/Layouts/MainMenuButtonRibbonHD.json"
        };

        public UserControl UI { get { return this; } }
        public Window _window;

        public Plugin()
        {
            using (var json = ReadResourceFile(PluginResources.Data))
            {
                using (var streamReader = new StreamReader(json))
                {
                    Settings = JsonConvert.DeserializeObject<Model.Settings>(streamReader.ReadToEnd());
                }
            }

            InitializeComponent();
        }

        public void Initialize(Window window)
        {
            _window = window;
        }

        public void Execute(ExecuteContext Context)
        {
            //creates corruption orb item type
            AddCorruptionMiscItemType(Context);

            //creates the hidden stat saying the item has been corrupted
            AddCorruptionStat(Context);
            AddCorruptionRollStat(Context);
            AddCorruptionDescStat(Context);

            AddCorruptionProperty(Context);
            AddCorruptionRollProperty(Context);
            AddCorruptionDescProperty(Context);
            //creates the item that will do the corrupting when cubed
            AddCorruptionMiscItem(Context);

            int MaxRecipes = GetMaxCorruptionRecipes(Context);

            if (Settings.Method == 1)
            {
                AddCorruptionUniqueItem(Context);

                AddCommonCubeRecipes(Context);
                AddCubeRecipesMethod1(Context, MaxRecipes);
                AddString(Context, @"data:data/local/lng/strings/item-names.json", new Dictionary<string, object>()
                {
                    { "id", Utils.GetNextStringID(Context) },
                    { "Key", $"{Settings.ItemName}" },
                    { "enUS", $"{Settings.ItemName}" },
                });
            } else
            {
                AddCorruptionUniqueItems(Context, MaxRecipes);
                //creates the cube recipes based on the corruptor stat roll
                AddCommonCubeRecipes(Context);
                AddCubeRecipesMethod2(Context);

                //add strings
                var file = Context.ModFiles[@"data:data/local/lng/strings/item-names.json"];
                JArray root = (JArray)Utils.ReadJSONFile(file);
                for (int i = 1; i <= MaxRecipes; i++)
                {
                    root.Add(JObject.FromObject(new Dictionary<string, object>()
                    {
                        { "id", Utils.GetNextStringID(Context) },
                        { "Key", $"{Settings.ItemName} {i.ToString("0")}" },
                        { "enUS", $"{Settings.ItemName} {i.ToString("0")}" },
                    }));
                }
                Utils.WriteJSONFile(file, root);
            }

            //add assets
            AddItemsJsonAsset(Context);

            //add strings
            AddString(Context, @"data:data/local/lng/strings/item-modifiers.json", new Dictionary<string, object>()
            {
                { "id", Utils.GetNextStringID(Context) },
                { "Key", "ModCorruptionDesc" },
                { "enUS", $"{D2RModMaker.Api.Constants.Colors.Yellow}Cube with an item to corrupted it." },
            });
            AddString(Context, @"data:data/local/lng/strings/item-modifiers.json", new Dictionary<string, object>()
            {
                { "id", Utils.GetNextStringID(Context) },
                { "Key", "ModCorruptedPosDesc" },
                { "enUS", $"{D2RModMaker.Api.Constants.Colors.Red}Corrupted" },
            });
            AddString(Context, @"data:data/local/lng/strings/item-modifiers.json", new Dictionary<string, object>()
            {
                { "id", Utils.GetNextStringID(Context) },
                { "Key", "ModCorruptedNegDesc" },
                { "enUS", $"{D2RModMaker.Api.Constants.Colors.Yellow}Corrupting - Cube again by itself to finished corruption." },
            });
            //only used for debugging
            AddString(Context, @"data:data/local/lng/strings/item-modifiers.json", new Dictionary<string, object>()
            {
                { "id", Utils.GetNextStringID(Context) },
                { "Key", "ModCorruptionRoll" },
                { "enUS", $"Rolled" },
            });
        }

        private void AddString(ExecuteContext Context, string File, Dictionary<string,object> Data)
        {
            var file = Context.ModFiles[File];
            JArray root = (JArray)Utils.ReadJSONFile(file);
            root.Add(JObject.FromObject(Data));

            Utils.WriteJSONFile(file, root);
        }

        private void AddItemsJsonAsset(ExecuteContext Context)
        {
            var file = Context.ModFiles[@"data:data/hd/items/items.json"];
            JArray root = (JArray)Utils.ReadJSONFile(file);

            root.Add(new JObject {
              new JProperty( $"{Settings.ItemCode}", new JObject { new JProperty ( "asset", "scroll/identify_scroll" ) } )
            });

            Utils.WriteJSONFile(file, root);
        }

        private void AddCorruptionMiscItemType(ExecuteContext Context)
        {
            var txt = TXTFile.Read(Context.ModFiles[@"data:data/global/excel/ItemTypes.txt"]);
            var row = txt.GetByColumnAndValue("Code", $"{Settings.ItemTypeCode}");
            if (row == null)
            {
                row = new TXTRow(txt.Columns, new string[txt.Columns.Count]);
                txt.Rows.Add(row);
            }

            row["ItemType"].Value = $"{Settings.ItemTypeName}";
            row["Code"].Value = $"{Settings.ItemTypeCode}";
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
            var row = txt.GetByColumnAndValue("Stat", "item_corrupted");
            if (row == null)
            {
                row = new TXTRow(txt.Columns, new string[txt.Columns.Count]);
                row["*ID"].Value = txt.NextID();
                txt.Rows.Add(row);
            } 
            row["Stat"].Value = "item_corrupted";
            row["Send Other"].Value = "";
            row["Signed"].Value = "1";
            row["Send Bits"].Value = "2";
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
            row["Save Add"].Value = "1";
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
            row["descval"].Value = "0";
            row["descstrpos"].Value = "ModCorruptedPosDesc";
            row["descstrneg"].Value = "ModCorruptedNegDesc";
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

        private void AddCorruptionDescStat(ExecuteContext Context)
        {
            var txt = TXTFile.Read(Context.ModFiles[@"data:data/global/excel/ItemStatCost.txt"]);
            var row = txt.GetByColumnAndValue("Stat", "item_corruption_desc");
            if (row == null)
            {
                row = new TXTRow(txt.Columns, new string[txt.Columns.Count]);
                row["*ID"].Value = txt.NextID();
                txt.Rows.Add(row);
            }
            row["Stat"].Value = "item_corruption_desc";
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
            row["Save Bits"].Value = "1";
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
            row["descval"].Value = "0";
            row["descstrpos"].Value = "ModCorruptionDesc";
            row["descstrneg"].Value = "ModCorruptionDesc";
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

        private void AddCorruptionRollStat(ExecuteContext Context)
        {
            var txt = TXTFile.Read(Context.ModFiles[@"data:data/global/excel/ItemStatCost.txt"]);
            var row = txt.GetByColumnAndValue("Stat", "item_corruption_roll");
            if (row == null)
            {
                row = new TXTRow(txt.Columns, new string[txt.Columns.Count]);
                row["*ID"].Value = txt.NextID();
                txt.Rows.Add(row);
            }
            row["Stat"].Value = "item_corruption_roll";
            row["Send Other"].Value = "";
            row["Signed"].Value = "";
            row["Send Bits"].Value = "11";
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
            row["Save Bits"].Value = "11";
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
            row["descpriority"].Value = "1";
            row["descfunc"].Value = "3";
            row["descval"].Value = "0";
            row["descstrpos"].Value = "ModCorruptionRoll";
            row["descstrneg"].Value = "ModCorruptionRoll";
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
            row["stat1"].Value = "item_corrupted";
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

        private void AddCorruptionDescProperty(ExecuteContext Context)
        {
            var txt = TXTFile.Read(Context.ModFiles[@"data:data/global/excel/Properties.txt"]);
            var row = txt.GetByColumnAndValue("code", "corruption-desc");
            if (row == null)
            {
                row = new TXTRow(txt.Columns, new string[txt.Columns.Count]);
                txt.Rows.Add(row);
            }
            row["code"].Value = "corruption-desc";
            row["*Enabled"].Value = "1";
            row["func1"].Value = "1";
            row["stat1"].Value = "item_corruption_desc";
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

        private void AddCorruptionRollProperty(ExecuteContext Context)
        {
            var txt = TXTFile.Read(Context.ModFiles[@"data:data/global/excel/Properties.txt"]);
            var row = txt.GetByColumnAndValue("code", "corruption-roll");
            if (row == null)
            {
                row = new TXTRow(txt.Columns, new string[txt.Columns.Count]);
                txt.Rows.Add(row);
            }
            row["code"].Value = "corruption-roll";
            row["*Enabled"].Value = "1";
            row["func1"].Value = "1";
            row["stat1"].Value = "item_corruption_roll";
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
            var row = txt.GetByColumnAndValue("name", $"{Settings.ItemName}");

            if (row == null)
            {
                row = new TXTRow(txt.Columns, new string[txt.Columns.Count]);
                txt.Rows.Add(row);
            }
            
            //all rows in an unmodified txt
            row["name"].Value = $"{Settings.ItemName}";
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
            row["code"].Value = $"{Settings.ItemCode}";
            row["alternategfx"].Value = "rsc";
            row["namestr"].Value = $"{Settings.ItemCode}";
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
            row["Transform"].Value = "0";
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

        private int GetMaxCorruptionRecipes(ExecuteContext Context)
        {
            int max = 0;
            //how many unq items do we need to make

            foreach (var entry in Settings.Corruptions)
            {
                int count = 0;
                count += (int)entry.Brick;
                foreach (var tier in entry.Tiers)
                {
                    int weight = (int)tier.Weight;
                    int mods = tier.Mods.Count;
                    count += weight * mods;
                }
                if (count > max)
                {
                    max = count;
                }
            }
            return max;
        }

        private void AddCorruptionUniqueItems(ExecuteContext Context, int MaxRecipes)
        {
            var txt = TXTFile.Read(Context.ModFiles[@"data:data/global/excel/UniqueItems.txt"]);


            for (int i = 1; i <= MaxRecipes; i++) { 
                AddOrEditUniqueItem(txt, new Dictionary<string, string>() {
                    { "index", $"{Settings.ItemName} {i.ToString("0")}" },
                    {"*ID", ""  },
                    {"rarity", "1" },
                    {"nolimit","1" },
                    { "lvl", "1"},
                    { "lvl req", "1" },
                    { "code", $"{Settings.ItemCode}" },
                    { "*ItemName", $"{Settings.ItemName} {i.ToString("0")}" },
                    { "cost mult", "5" },
                    { "cost add", "5000" },
                    { "invtransform", "cred" },
                    {  "prop1", "corruption-desc" },
                    { "min1", "1" },
                    { "max1", "1" },
                });
            }

            txt.Write();
        }

        private void AddCorruptionUniqueItem(ExecuteContext Context)
        {
            var txt = TXTFile.Read(Context.ModFiles[@"data:data/global/excel/UniqueItems.txt"]);

            
            AddOrEditUniqueItem(txt, new Dictionary<string, string>() {
                { "index", $"{Settings.ItemName}" },
                {"*ID", ""  },
                {"rarity", "1" },
                {"nolimit","1" },
                { "lvl", "1"},
                { "lvl req", "1" },
                { "code", $"{Settings.ItemCode}" },
                { "*ItemName", $"{Settings.ItemName}" },
                { "cost mult", "5" },
                { "cost add", "5000" },
                { "invtransform", "cred" },
                {  "prop1", "corruption-desc" },
                { "min1", "1" },
                { "max1", "1" }
            });

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

        private string GetTierStats(Model.Corruption entry)
        {
            var brick = entry.Brick;
            var tierCount = entry.Tiers.Count;
            var tierCounts = new int[tierCount];
            var total = brick;
            for (int tierIdx = 0; tierIdx < tierCount; tierIdx++)
            {
                var tier = entry.Tiers[tierIdx];
                var weight = tier.Weight;
                tierCounts[tierIdx] = tier.Mods.Count * (int)weight;
                total += tierCounts[tierIdx];
            }
            var strings = new string[tierCount];
            for (int tierIdx = 0; tierIdx < tierCount; tierIdx++)
            {
                strings[tierIdx] = $"tier{tierIdx + 1}={(100.0 * (double)tierCounts[tierIdx] / (double)total).ToString("F")}";
            }
            return string.Join(",", strings) + $",brick={(100.0 * (double)brick / (double)total).ToString("F")}";
        }

        private void AddCommonCubeRecipes(ExecuteContext Context)
        {
            var txt = TXTFile.Read(Context.ModFiles[@"data:data/global/excel/cubemain.txt"]);

            //testing recipes
            AddOrEditCubeMainRecipe(txt, new Dictionary<string, string>() {
                { "description", "The Gnasher"},
                {"numinputs", "1" },
                {"input 1", "isc" },
                {"output","The Gnasher" },
                { "lvl", "99"}
            });

            AddOrEditCubeMainRecipe(txt, new Dictionary<string, string>() {
                { "description", $"{Settings.ItemName}"},
                {"numinputs", "1" },
                {"input 1", "hp1" },
                {"output",$"{Settings.ItemCode},uni" },
                //{ "lvl", "99"},
            });

            //these have issues
            //real recipes
            AddOrEditCubeMainRecipe(txt, new Dictionary<string, string>() {
                { "description", $"Unique Item + Any Potion -> {Settings.ItemName}"},
                {"numinputs", "2" },
                {"input 1", "uni" },
                {"input 2", "poti" },
                {"output",$"{Settings.ItemCode},uni" },
                //{ "lvl", "99"},
            });
            AddOrEditCubeMainRecipe(txt, new Dictionary<string, string>() {
                { "description", $"Set Item + Any Potion -> {Settings.ItemName}"},
                {"numinputs", "2" },
                {"input 1", "set" },
                {"input 2", "poti" },
                {"output",$"{Settings.ItemCode},uni" },
                //{ "lvl", "99"},
            });

            txt.Write();
        }

        private void AddCubeRecipesMethod1(ExecuteContext Context, int MaxRecipes)
        {
            var txt = TXTFile.Read(Context.ModFiles[@"data:data/global/excel/cubemain.txt"]);
            var isctxt = TXTFile.Read(Context.ModFiles[@"data:data/global/excel/ItemStatCost.txt"]);
            var item_corruption_roll = isctxt.GetByColumnAndValue("Stat", "item_corruption_roll");
            var item_corrupted = isctxt.GetByColumnAndValue("Stat", "item_corrupted");
            var item_corruption_desc = isctxt.GetByColumnAndValue("Stat", "item_corruption_desc");

            //iterate data.json to build all recipes
            foreach (var entry in Settings.Corruptions)
            {
                var type = entry.Name;
                var dictionary = new Dictionary<string, string>() {
                    { "op", "18" },
                    { "param", item_corruption_roll["*ID"].Value },
                    {"numinputs", "1" },
                    {"output","useitem" },
                    //{ "lvl", "99" },
                    {"mod 1","corruption" },
                    {"mod 1 min", "2" },
                    {"mod 1 max", "2" },
                    //make roll out of range
                    {"mod 2","corruption-roll" },
                    {"mod 2 min", MaxRecipes.ToString("0") },
                    {"mod 2 max", MaxRecipes.ToString("0") },
                };
                foreach (var quality in entry.Qualities)
                {
                    int i = 1;
                    var brick = entry.Brick;
                    var tierCount = entry.Tiers.Count;

                    AddOrEditCubeMainRecipe(txt, new Dictionary<string, string>() {
                        { "description", $"### Tier Stats ({type},{quality},{ GetTierStats(entry) }) ###"},
                        {"enabled", "0" }
                    });

                    AddOrEditCubeMainRecipe(txt, new Dictionary<string, string>() {
                        { "description", $"Add Corruption Roll ({type},{quality})"},
                        { "op", "18" },
                        { "param", item_corrupted["*ID"].Value },
                        { "value", "0" },
                        {"numinputs", "2" },
                        {"input 1", $"{type},{quality}" },
                        {"input 2", $"{Settings.ItemName}" },
                        {"output","useitem" },
                        {"mod 1", "corruption" },
                        {"mod 1 min", "-1" },
                        {"mod 1 max", "-1" },
                        {"mod 2","corruption-roll" },
                        {"mod 2 min", "1" },
                        {"mod 2 max", MaxRecipes.ToString("0") },
                    });

                    dictionary["input 1"] = $"{type},{quality}";
                    for (int tierIdx = 0; tierIdx < tierCount; tierIdx++)
                    {
                        var tier = entry.Tiers[tierIdx];
                        var weight = (int)tier.Weight;
                        int count = tier.Mods.Count * weight;
                        for (int j = 0; j < count; j++)
                        {
                            dictionary["description"] = $"Corrupt {type},{quality},tier={tierIdx + 1}";
                            dictionary["value"] = $"{(i++).ToString("0")}";
                            var mods = tier.Mods[j / weight];
                            for (int k = 0; k < 4; k++)
                            {
                                dictionary[$"mod 3"] = mods.Stat1;
                                dictionary[$"mod 3 chance"] = mods.Chance1;
                                dictionary[$"mod 3 param"] = mods.Param1;
                                dictionary[$"mod 3 min"] = mods.Min1;
                                dictionary[$"mod 3 max"] = mods.Max1;
                                dictionary[$"mod 4"] = mods.Stat2;
                                dictionary[$"mod 4 chance"] = mods.Chance2;
                                dictionary[$"mod 4 param"] = mods.Param2;
                                dictionary[$"mod 4 min"] = mods.Min2;
                                dictionary[$"mod 4 max"] = mods.Max2;
                                dictionary[$"mod 5"] = mods.Stat3;
                                dictionary[$"mod 5 chance"] = mods.Chance3;
                                dictionary[$"mod 5 param"] = mods.Param3;
                                dictionary[$"mod 5 min"] = mods.Min3;
                                dictionary[$"mod 5 max"] = mods.Max3;
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
                        dictionary["value"] = $"{(i++).ToString("0")}";
                        var output = Constants.BrickQualities[quality];
                        if (string.IsNullOrEmpty(output))
                        {
                            //eat the item
                            dictionary["output"] = $"";
                        }
                        else
                        {
                            dictionary["output"] = $"usetype,{output}";
                        }
                        AddOrEditCubeMainRecipe(txt, dictionary);
                    }
                }
            }

            txt.Write();
        }

        private void AddCubeRecipesMethod2(ExecuteContext Context)
        {
            var txt = TXTFile.Read(Context.ModFiles[@"data:data/global/excel/cubemain.txt"]);
            var isctxt = TXTFile.Read(Context.ModFiles[@"data:data/global/excel/ItemStatCost.txt"]);
            var item_corruption_roll = isctxt.GetByColumnAndValue("Stat", "item_corruption_roll");
            var item_corrupted = isctxt.GetByColumnAndValue("Stat", "item_corrupted");
            var item_corruption_desc = isctxt.GetByColumnAndValue("Stat", "item_corruption_desc");

            //testing recipes
            AddOrEditCubeMainRecipe(txt, new Dictionary<string, string>() {
                { "description", "The Gnasher"},
                {"numinputs", "1" },
                {"input 1", "isc" },
                {"output","The Gnasher" },
                { "lvl", "99"}
            });

            AddOrEditCubeMainRecipe(txt, new Dictionary<string, string>() {
                { "description", $"{Settings.ItemName}"},
                {"numinputs", "1" },
                {"input 1", "hp1" },
                {"output",$"{Settings.ItemCode},uni" },
                //{ "lvl", "99"},
            });

            //these have issues
            //real recipes
            AddOrEditCubeMainRecipe(txt, new Dictionary<string, string>() {
                { "description", $"Unique Item + Any Potion -> {Settings.ItemName}"},
                {"numinputs", "2" },
                {"input 1", "uni" },
                {"input 2", "poti" },
                {"output",$"{Settings.ItemCode},uni" },
                //{ "lvl", "99"},
            });
            AddOrEditCubeMainRecipe(txt, new Dictionary<string, string>() {
                { "description", $"Set Item + Any Potion -> {Settings.ItemName}"},
                {"numinputs", "2" },
                {"input 1", "set" },
                {"input 2", "poti" },
                {"output",$"{Settings.ItemCode},uni" },
                //{ "lvl", "99"},
            });

            //iterate data.json to build all recipes
            foreach(var entry in Settings.Corruptions)
            {
                var type = entry.Name;
                var dictionary = new Dictionary<string, string>() {
                    //only allow if not already corrupted
                    { "op", "17" },
                    { "param", item_corrupted["*ID"].Value },
                    { "value", "1" },

                    {"numinputs", "2" },
                    {"output","useitem" },
                    //{ "lvl", "99" },
                    {"mod 1","corruption" },
                    {"mod 1 min", "1" },
                    {"mod 1 max", "1" },
                };
                foreach(var quality in entry.Qualities)
                {
                    int i = 1;
                    var brick = entry.Brick;
                    var tierCount = entry.Tiers.Count;

                    AddOrEditCubeMainRecipe(txt, new Dictionary<string, string>() {
                        { "description", $"### Tier Stats ({type},{quality},{ GetTierStats(entry) }) ###"},
                        {"enabled", "0" }
                    });

                    dictionary["input 1"] = $"{type},{quality}";
                    for (int tierIdx = 0; tierIdx < tierCount; tierIdx++)
                    {
                        var tier = entry.Tiers[tierIdx];
                        var weight = (int)tier.Weight;
                        int count = tier.Mods.Count * weight;
                        for (int j = 0; j < count; j++)
                        {
                            dictionary["description"] = $"Corrupt {type},{quality},tier={tierIdx+1}";
                            dictionary["input 2"] = $"{Settings.ItemName} {(i++).ToString("0")}";
                            var mods = tier.Mods[j / weight];
                            for (int k = 0; k < 4; k++)
                            {
                                dictionary[$"mod 3"] = mods.Stat1;
                                dictionary[$"mod 3 chance"] = mods.Chance1;
                                dictionary[$"mod 3 param"] = mods.Param1;
                                dictionary[$"mod 3 min"] = mods.Min1;
                                dictionary[$"mod 3 max"] = mods.Max1;
                                dictionary[$"mod 4"] = mods.Stat2;
                                dictionary[$"mod 4 chance"] = mods.Chance2;
                                dictionary[$"mod 4 param"] = mods.Param2;
                                dictionary[$"mod 4 min"] = mods.Min2;
                                dictionary[$"mod 4 max"] = mods.Max2;
                                dictionary[$"mod 5"] = mods.Stat3;
                                dictionary[$"mod 5 chance"] = mods.Chance3;
                                dictionary[$"mod 5 param"] = mods.Param3;
                                dictionary[$"mod 5 min"] = mods.Min3;
                                dictionary[$"mod 5 max"] = mods.Max3;
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
                        dictionary["input 2"] = $"{Settings.ItemName} {(i++).ToString("0")}";
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var select = (Model.Corruption)CorruptionListView.SelectedItem;
            if(select == null)
            {
                return;
            }
            Editor editor = new Editor(select);
            _window.Closed += (object sender, EventArgs e) => editor.Close();
            editor.Closed += (object sender, EventArgs e) =>
            {
                select = editor.Corruption;
            };
            editor.Show();

        }
    }
}
