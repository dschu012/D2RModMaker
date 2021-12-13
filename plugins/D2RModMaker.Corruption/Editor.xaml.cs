using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace D2RModMaker.Corruption
{
    /// <summary>
    /// Interaction logic for Editor.xaml
    /// </summary>
    public partial class Editor : Window
    {

        public Model.Corruption Corruption { get; set; }

        public Editor(Model.Corruption corruption)
        {
            Corruption = corruption;
            InitializeComponent();
            RefreshStatistics();
        }

        private void RefreshStatistics()
        {
            var brick = Corruption.Brick;
            var tierCount = Corruption.Tiers.Count;
            var tierCounts = new int[tierCount];
            var total = brick;
            for (int tierIdx = 0; tierIdx < tierCount; tierIdx++)
            {
                var tier = Corruption.Tiers[tierIdx];
                var weight = tier.Weight;
                tierCounts[tierIdx] = tier.Mods.Count * (int)weight;
                total += tierCounts[tierIdx];
            }
            var strings = new string[tierCount];
            for (int tierIdx = 0; tierIdx < tierCount; tierIdx++)
            {
                var tier = Corruption.Tiers[tierIdx];
                strings[tierIdx] = $"tier{tierIdx + 1}={(100.0 * (double)tierCounts[tierIdx] / (double)total).ToString("F")}%\tCubemain Rows = {tierCounts[tierIdx]} ( Weight({tier.Weight}) * Mods({tier.Mods.Count}) )";
            }
            StatisticTextBlock.Text = string.Join("\n", strings) + $"\nbrick={(100.0 * (double)brick / (double)total).ToString("F")}%\tCubemain Rows = {Corruption.Brick}";
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            RefreshStatistics();
        }
    }
}
