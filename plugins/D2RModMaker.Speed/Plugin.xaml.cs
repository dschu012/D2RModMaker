using D2RModMaker.Api;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Dynamic;
using System.Linq;
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

namespace D2RModMaker.Speed
{
    /// <summary>
    /// Interaction logic for Plugin.xaml
    /// </summary>
    [Export(typeof(IPlugin))]
    public partial class Plugin : UserControl, IPlugin
    {

        public string PluginName { get; set; } = "Speed";
        public bool Enabled { get; set; } = true;
        public dynamic Settings { get; set; } = new ExpandoObject();
        public int DisplayOrder { get; } = int.MinValue;
        public int ExecutionOrder { get; } = int.MaxValue;

        public string[] RequiredFiles { get; } = new string[]
        {
            @"data:data/global/excel/charstats.txt",
            @"data:data/global/excel/monstats.txt",
            @"data:data/global/excel/Missiles.txt",
        };

        public UserControl UI { get { return this; } }


        public Plugin()
        {
            Settings.PlayerSpeed = 1.0;
            Settings.MonsterSpeed = 1.0;
            Settings.MissileSpeed = 1.0;
            InitializeComponent();
        }

        public void Initialize(Window window) { }

        public void Execute(ExecuteContext Context)
        {
            var basecharstatstxt = TXTFile.Read(Context.UnmodifiedFiles[@"data:data/global/excel/charstats.txt"]);
            var charstatstxt = TXTFile.Read(Context.ModFiles[@"data:data/global/excel/charstats.txt"]);
            foreach (var row in charstatstxt.Rows)
            {
                var baserow = basecharstatstxt.GetByColumnAndValue("class", row["class"].Value);
                if (baserow == null)
                {
                    continue;
                }
                row["WalkVelocity"].Value = (baserow["WalkVelocity"].ToUInt32() * Settings.PlayerSpeed).ToString("0");
                row["RunVelocity"].Value = (baserow["RunVelocity"].ToUInt32() * Settings.PlayerSpeed).ToString("0");
            }
            charstatstxt.Write();

            var basemonstattxt = TXTFile.Read(Context.UnmodifiedFiles[@"data:data/global/excel/monstats.txt"]);
            var monstattxt = TXTFile.Read(Context.ModFiles[@"data:data/global/excel/monstats.txt"]);
            foreach (var row in monstattxt.Rows)
            {
                var baserow = basemonstattxt.GetByColumnAndValue("Id", row["Id"].Value);
                if (baserow == null)
                {
                    continue;
                }
                row["Velocity"].Value = (baserow["Velocity"].ToUInt32() * Settings.MonsterSpeed).ToString("0");
                row["Run"].Value = (baserow["Run"].ToUInt32() * Settings.MonsterSpeed).ToString("0");
            }
            monstattxt.Write();

            var basemissilestxt = TXTFile.Read(Context.UnmodifiedFiles[@"data:data/global/excel/Missiles.txt"]);
            var missilestxt = TXTFile.Read(Context.ModFiles[@"data:data/global/excel/Missiles.txt"]);
            foreach (var row in missilestxt.Rows)
            {
                var baserow = basemissilestxt.GetByColumnAndValue("*ID", row["*ID"].Value);
                if (baserow == null)
                {
                    continue;
                }
                row["Vel"].Value = (baserow["Vel"].ToUInt32() * Settings.PlayerSpeed).ToString("0");
                row["MaxVel"].Value = (baserow["MaxVel"].ToUInt32() * Settings.PlayerSpeed).ToString("0");
            }
            missilestxt.Write();
        }
    }
}
