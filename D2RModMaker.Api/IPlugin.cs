using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace D2RModMaker.Api
{

    public interface IPlugin
    {
        bool Enabled { get; set; }
        string PluginName { get; set; }
        dynamic Settings { get; set; }
        UserControl UI { get; }
        string[] RequiredFiles { get; }

        void Execute(ExecuteContext Context);


    }
}
