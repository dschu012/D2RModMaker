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

namespace D2RModMaker.DropRates
{
    /// <summary>
    /// Interaction logic for Plugin.xaml
    /// </summary>
    [Export(typeof(IPlugin))]
    public partial class Plugin : UserControl, IPlugin
    {
        public string PluginName { get; set; } = "Drop Rates";
        public bool Enabled { get; set; } = true;
        public dynamic Settings { get; set; } = new ExpandoObject();
        public int DisplayOrder { get; } = int.MinValue;
        public int ExecutionOrder { get; } = int.MinValue;
        public UserControl UI { get { return this; } }

        public string[] RequiredFiles { get; } = new string[]
        {
            @"data:data/global/excel/TreasureClassEx.txt",
        };

        public Plugin()
        {
            Settings.NoDropZero = true;
            InitializeComponent();
        }

        public void Initialize(Window window) { }

        public void Execute(ExecuteContext Context)
        {
            if(Settings.NoDropZero)
            {
                NoDropZero(Context);
            }
        }

        private void NoDropZero(ExecuteContext Context)
        {
            var txt = TXTFile.Read(Context.ModFiles[@"data:data/global/excel/TreasureClassEx.txt"]);
            foreach (var row in txt.Rows)
            {
                row["NoDrop"].Value = "0";
            }
        }
    }
}
