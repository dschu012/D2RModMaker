using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace D2RModMaker.Api
{
    public class ModInfo
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("savepath")]
        public string SavePath { get; set; }
        [JsonProperty("args")]
        public string Args { get; set; }
        [JsonProperty("plugins")]
        public Dictionary<string, dynamic> Plugins { get; set; } = new Dictionary<string, dynamic>();

    }
}
