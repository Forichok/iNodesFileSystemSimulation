using System;
using System.Collections.Generic;
using System.Linq;

namespace Tanenbaum_4_48
{
    class DataBlock
    {
        public int Id { get; }
        private string _data;
        private int Size { get; }

        public bool IsFree { get; }

        public DataBlock(int id,int size,string data,bool isFree =true)
        {
            Id = id;
            Size = size;
            _data = data;
            IsFree = isFree;
        }

        public string GetData()
        {
            return _data;
        }

        public  Dictionary<string,int> GetDirectoryInfo()
        {
            var directories = new Dictionary<string, int>();
            _data = _data.Trim('*');
            var a = _data.Split('\n').ToList();
            a.Remove(a.Last());
            foreach (var line in a)
            {
                var splitedLine = line.Split('|');
                directories.Add(splitedLine[0],Convert.ToInt32(splitedLine[1]));
            }
            return directories;
        }

        public static DataBlock GetEmptyDatablock(int id,int size)
        {
            return new DataBlock(id,size,"");
        }

        public int GetNodeId(string name)
        {
            _data = _data.Trim('*');
            var a = _data.Split('\n').ToList();
            a.Remove(a.Last());
            foreach (var item in a)
            {
                var b = item.Split('|').ToList();
                if (b[0].Trim() == name.Trim())
                    return Convert.ToInt32(b[1]);
            }
            return 0;
        }

        public void Add (int freeInodeId,string name)
        {
            _data = _data.Trim('*');
            _data += $"{name} | {freeInodeId}\n";

        }
        public void Remove(string name,int nodeId)
        {
            _data = _data.Trim('*');
            var splited = _data.Split('\n').ToList();
            foreach (var item in splited)
            {
                if (item.Split('|')[0].Trim() == name.Trim()
                    && item.Split('|')[1].Trim() == nodeId.ToString())
                {
                    splited.Remove(item);
                    break;
                }
            }
            _data =string.Join("\n",splited);
        }

        public override string ToString()
        {
            var output = "\n";

            if (IsFree)
                output += "free ";

            output+=$"Data Block : {Id}".PadRight(34,'=')+'\n';
            output += _data;

            output = output.PadRight(Size, '*');
            return output;
        }
    }
}
