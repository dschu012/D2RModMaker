using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace D2RModMaker.Api
{
    public static class StreamUtils
    {

        public static void WriteStreamToFile(Stream s, string path)
        {
            if(s == null)
            {
                return;
            }
            string dir = Path.GetDirectoryName(path);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            using (FileStream f = new FileStream(path, FileMode.Create, FileAccess.ReadWrite))
            {
                s.CopyTo(f);
            }
        }

        public static JObject ReadJSONFile(string path)
        {
            using (StreamReader sr = File.OpenText(path))
            {
                return JObject.Parse(sr.ReadToEnd());
            }
        }

        public static void WriteJSONFile(string path, JObject json)
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
