using System.Collections.Generic;

namespace D2RModMaker.Corruption
{
    public class PluginResources
    {
        public static readonly string Data = "D2RModMaker.Corruption.Data.json";
    }
    public class Constants
    {
        public static readonly List<string> Qualities = new List<string>()
        {
            "uni", "set", "crf", "rar", "mag", "hig", "nor", "low"
        };

        public static readonly Dictionary<string,string> BrickQualities = new Dictionary<string, string>()
        {
            { "uni", "rar" },
            { "set", "rar" },
            { "crf", "mag" },
            { "rar", "mag" },
            { "mag", "nor" },
            { "hiq", "low" },
            { "nor", "low" },
            { "low", "" },
        };
    }
}
