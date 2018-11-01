using System;
using System.Collections.Generic;
using System.Linq;

namespace Tanenbaum_4_48
{
    class INode
    {
        public int Id { get; }
        public string Type { set; get; }
        public string Mode { set; get; }
        public int LinkCount { set; get; }
        public string User { set; get; }
        public string Group { set; get; }
        public int Size { set; get; }
        public DateTime TimeStamp { set; get; }
        public List<int> DataBlocks { get; }

        public INode(int id, string type, string mode, int linkCount, string user, string group, int size, DateTime timeStamp, List<int> dataBlocks)
        {
            Id = id;
            Type = type ?? "null";
            Mode = mode ?? "null";
            LinkCount = linkCount;
            User = user ?? "null";
            Group = group ?? "null";
            Size = size;
            TimeStamp = timeStamp;
            DataBlocks = dataBlocks;
        }

        public static INode GetEmptyNode(int id,int inodeSize)
        {
            return new INode(id, null, null, 0, null, null, inodeSize, DateTime.MinValue, new List<int>());
        }



        public static INode GetNode(string iNodeInfo)
        {
            var lines = iNodeInfo.Split('\n').Select(s => s.Trim(' ', '=')).Except(new[] { "" }).ToList();
            if (lines[8].Split(new[] {" : "}, StringSplitOptions.RemoveEmptyEntries).Length == 1)
                lines[8] += " 0";
            var values = lines.Select(s => s.Split(new[] { " : " }, StringSplitOptions.RemoveEmptyEntries)[1]).ToList();
            var dataBlocks = values[8]=="0"?new List<int>() : values[8].Split(' ').Select(s => Convert.ToInt32(s)).ToList();
            var inode = new INode(Convert.ToInt32(values[0]),values[1], values[2], Convert.ToInt32(values[3]), values[4], values[5], Convert.ToInt32(values[6]), DateTime.Parse(values[7]), dataBlocks);
            return inode;
        }

        
        public void AddDataBlock(int id)
        {
            DataBlocks.Add(id);
        }

        public override string ToString()
        {
            string output = null;

            output += $"Id : {Id}".PadRight(10) + '\n';
            output += $"Type : {Type}".PadRight(20) + '\n';
            output += $"Mode : {Mode}".PadRight(10) + '\n';
            output += $"Link Count : {LinkCount}".PadRight(10) + '\n';
            output += $"User : {User}".PadRight(30) + '\n';
            output += $"Group : {Group}".PadRight(30) + '\n';
            output += $"Size : {Size}".PadRight(10) + '\n';
            output += $"TimeStamp : {TimeStamp}".PadRight(30) + '\n';
            output += $"DataBlocks : {string.Join(" ",DataBlocks)}".PadRight(40) + '\n';
            
            output = output.PadRight(FileSystem._inodeSize-1,'=')+'\n';

            return output;
        }
    }
}
