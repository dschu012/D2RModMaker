using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace D2RModMaker.Api
{
    public interface IPlugin
    {
        string PluginName { get; set; }
        dynamic Settings { get; set; }
        UserControl UI { get; }
        string[] RequiredFiles { get; }

        void Execute(ExecuteContext Context);


    }
}
