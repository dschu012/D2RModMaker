using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace D2RModMaker.Api
{
    public class ExecuteContext
    {

        //unmodified files from d2r. i.e. if u wanted to scale monsters by a ratio you'd do calculations off of this file.
        //mapping of data:data/global/excel/inventory.txt to wherever it is temporarily extracted in filesystem.
        public Dictionary<string, string> UnmodifiedFiles;

        public string ModPath;

        //files in the mod directory
        //mapping of data:data/global/excel/inventory.txt to wherever it is temporarily extracted in filesystem.
        public Dictionary<string, string> ModFiles;

        public Random Random;
    }
}
