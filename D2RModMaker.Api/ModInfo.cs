using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace D2RModMaker.Api
{
    public class ModInfo
    {
        public string Name { get; set; }
        public string SavePath { get; set; }
        public string Path { get; set; }
        public string[] Files { get; set; }
    }
}
