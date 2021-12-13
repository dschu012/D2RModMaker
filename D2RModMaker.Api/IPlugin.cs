using System;
using System.Collections.Generic;
using System.Windows;
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

        //Order Sorted in the UI. Higher number ordered first.
        int DisplayOrder { get; }

        //Order txt changes get executed. Higher number goes first.
        int ExecutionOrder { get; }

        void Initialize(Window window);
        void Execute(ExecuteContext Context);


    }
}
