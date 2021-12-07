using D2RModMaker.Api;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Dynamic;
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
        public dynamic Settings { get; set; } = new ExpandoObject();

        public string[] RequiredFiles { get; } = new string[]
        {
            @"data:data/global/excel/Levels.txt",
            @"data:data/global/excel/monstats.txt"
        };

        public UserControl UI { get { return this; } }


        public Plugin()
        {
            Settings.MonsterQuanityScale = 1.0;
            Settings.HPScale = 1.0;
            Settings.XPScale = 1.0;
            InitializeComponent();
        }

        public void Execute(ExecuteContext Context)
        {
            var baselevelstxt = TXTFile.Read(Context.UnmodifiedFiles[@"data:data/global/excel/Levels.txt"]);
            var levelstxt = TXTFile.Read(Context.ModFiles[@"data:data/global/excel/Levels.txt"]);
            foreach (var row in levelstxt.Rows)
            {
                var baserow = baselevelstxt.GetByColumnAndValue("Id", row["Id"].Value);
                if(baserow == null)
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

            var basemonstattxt = TXTFile.Read(Context.UnmodifiedFiles[@"data:data/global/excel/monstats.txt"]);
            var monstattxt = TXTFile.Read(Context.ModFiles[@"data:data/global/excel/monstats.txt"]);
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

                row["Exp"].Value = (baserow["Exp"].ToUInt32() * Settings.XPScale).ToString("0");
                row["Exp(N)"].Value = (baserow["Exp(N)"].ToUInt32() * Settings.XPScale).ToString("0");
                row["Exp(H)"].Value = (baserow["Exp(H)"].ToUInt32() * Settings.XPScale).ToString("0");

            }
            monstattxt.Write();
        }
    }
}
