using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace D2RModMaker.Api
{
    public class TXTFile
    {
        public string Path { get; set; }
        public Dictionary<string, int> Columns { get; set; }
        public List<TXTRow> Rows { get; set; }

        public static TXTFile Read(string path, ISet<string> additionalColumns = null)
        {
            var txt = new TXTFile();
            txt.Path = path;
            txt.Columns = new Dictionary<string, int>();
            txt.Rows = new List<TXTRow>();
            using (StreamReader reader = new StreamReader(path))
            {
                //skip header
                int idx = 0;
                var columns = reader.ReadLine().Split('\t');
                foreach (var col in columns)
                {
                    if (txt.Columns.ContainsKey(col)) continue;
                    txt.Columns.Add(col, idx++);
                }
                if(additionalColumns != null)
                {
                    foreach(var col in additionalColumns)
                    {
                        if (txt.Columns.ContainsKey(col)) continue;
                        txt.Columns.Add(col, idx++);
                    }
                }
                while (reader.Peek() >= 0)
                {
                    var data = reader.ReadLine().Split('\t', txt.Columns.Count);
                    if(data.Length < txt.Columns.Count)
                    {
                        Array.Resize(ref data, txt.Columns.Count);
                    }
                    txt.Rows.Add(new TXTRow(txt.Columns, data));
                }
            }
            return txt;
        }

        public void Write()
        {
            using(StreamWriter writer = new StreamWriter(Path))
            {
                writer.WriteLine(string.Join("\t", Columns.Select(s => s.Key)));
                foreach(var row in Rows)
                {
                    writer.WriteLine(string.Join("\t", row.Data.Select(s => s.Value)));
                }
            }
        }

        public TXTRow GetByColumnAndValue(string name, string value)
        {
            foreach (var row in Rows)
            {
                if (row[name].Value.Trim() == value.Trim())
                    return row;
            }
            return null;
        }

    }

    public class TXTRow
    {
        public Dictionary<string, int> Columns { get; set; }
        public TXTCell[] Data { get; set; }

        public TXTCell this[int i] => this.GetByIndex(i);
        public TXTCell this[string i] => this.GetByColumn(i);

        public TXTRow(Dictionary<string, int> columns, string[] data)
        {
            Columns = columns;
            Data = data.Select(e => new TXTCell(e)).ToArray();
        }

        public TXTCell GetByIndex(int idx)
        {
            return Data[idx];
        }

        public TXTCell GetByColumn(string col)
        {
            return GetByIndex(Columns[col]);
        }
    }

    public class TXTCell
    {
        public string Value { get; set; }

        public Int32 ToInt32()
        {
            Int32 ret = 0;
            Int32.TryParse(Value, out ret);
            return ret;
        }

        public UInt32 ToUInt32()
        {
            UInt32 ret = 0;
            UInt32.TryParse(Value, out ret);
            return ret;
        }

        public UInt16 ToUInt16()
        {
            UInt16 ret = 0;
            UInt16.TryParse(Value, out ret);
            return ret;
        }

        public Int16 ToInt16()
        {
            Int16 ret = 0;
            Int16.TryParse(Value, out ret);
            return ret;
        }

        public bool ToBool()
        {
            return ToInt32() == 1;
        }
        public TXTCell(string value)
        {
            if(value == null)
            {
                value = "";
            }
            Value = value;
        }
    }
}
