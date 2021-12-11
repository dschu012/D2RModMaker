using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace D2RModMaker.Corruption
{
    public static class Model
    {

        public class Settings
        {
            [JsonProperty("itemname")]
            public string ItemName { get; set; }

            [JsonProperty("itemcode")]
            public string ItemCode { get; set; }

            [JsonProperty("itemtypename")]
            public string ItemTypeName { get; set; }

            [JsonProperty("itemtypecode")]
            public string ItemTypeCode { get; set; }

            [JsonProperty("statid")]
            public string StatId { get; set; }

            [JsonProperty("corruptions")]
            public List<Corruption> Corruptions { get; set; }
        }

        public class Corruption
        {
            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("label")]
            public string Label { get; set; }

            [JsonProperty("brick")]
            public double Brick { get; set; }

            [JsonProperty("qualities")]
            public ISet<string> Qualities { get; set; }

            [JsonProperty("tiers")]
            public List<Tier> Tiers { get; set; }
        }

        public class Tier
        {
            [JsonProperty("weight")]
            public double Weight { get; set; }

            [JsonProperty("mods")]
            public List<Mod> Mods { get; set; }
        }

        public class Mod
        {
            [JsonProperty("stat1")]
            public string Stat1 { get; set; }

            [JsonProperty("chance1")]
            public string Chance1 { get; set; }

            [JsonProperty("param1")]
            public string Param1 { get; set; }

            [JsonProperty("min1")]
            public string Min1 { get; set; }

            [JsonProperty("max1")]
            public string Max1 { get; set; }

            [JsonProperty("stat2")]
            public string Stat2 { get; set; }

            [JsonProperty("chance2")]
            public string Chance2 { get; set; }

            [JsonProperty("param2")]
            public string Param2 { get; set; }

            [JsonProperty("min2")]
            public string Min2 { get; set; }

            [JsonProperty("max2")]
            public string Max2 { get; set; }

            [JsonProperty("stat3")]
            public string Stat3 { get; set; }

            [JsonProperty("chance3")]
            public string Chance3 { get; set; }

            [JsonProperty("param3")]
            public string Param3 { get; set; }

            [JsonProperty("min3")]
            public string Min3 { get; set; }

            [JsonProperty("max3")]
            public string Max3 { get; set; }

            [JsonProperty("stat4")]
            public string Stat4 { get; set; }

            [JsonProperty("chance4")]
            public string Chance4 { get; set; }

            [JsonProperty("param4")]
            public string Param4 { get; set; }

            [JsonProperty("min4")]
            public string Min4 { get; set; }

            [JsonProperty("max4")]
            public string Max4 { get; set; }
        }

    }
}
