using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace D2RModMaker.Api
{
    public static class Utils
    {

        //pick a large number to start strings at so dont step on blizz's feet
        private static int NextStringID = 40000;

        public static int[] GetNextStringIDs(ExecuteContext Context, int count)
        {
            int[] ids = Enumerable.Range(NextStringID, NextStringID + count).ToArray();
            NextStringID += count;
            return ids;
        }
        public static int GetNextStringID(ExecuteContext Context)
        {
            return NextStringID++;
        }

        public static void Init(ExecuteContext Context)
        {
            string[] txt = File.ReadAllLines(Context.ModFiles[Constants.Files.NEXT_STRING_ID]);
            //NextStringID = int.Parse(txt[4]) + 1;
        }

        public static void Cleanup(ExecuteContext Context)
        {
            //string[] txt = File.ReadAllLines(Context.ModFiles[STRING_ID_FILE]);
            //txt[4] = NextStringID.ToString("0");
            //File.WriteAllLines(Context.ModFiles[STRING_ID_FILE], txt);
        }

        public static void WriteStreamToFile(Stream s, string path)
        {
            string dir = Path.GetDirectoryName(path);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            using (FileStream f = new FileStream(path, FileMode.Create, FileAccess.ReadWrite))
            {
                s.CopyTo(f);
            }
        }

        public static JToken ReadJSONFile(string path)
        {
            using (StreamReader sr = File.OpenText(path))
            {
                return JToken.Parse(sr.ReadToEnd());
            }
        }

        public static void WriteJSONFile(string path, JToken json)
        {
            string dir = Path.GetDirectoryName(path);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            using (var stream = File.Open(path, FileMode.Create, FileAccess.ReadWrite))
            {
                using (var sw = new StreamWriter(stream))
                {
                    using (JsonTextWriter jw = new JsonTextWriter(sw))
                    {
                        jw.Formatting = Formatting.Indented;
                        json.WriteTo(jw);
                    }
                }
            }
        }
    }
}
