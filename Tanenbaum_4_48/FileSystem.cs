using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Tanenbaum_4_48
{
    internal sealed class FileSystem
    {
        private int _inodesCount;
        private int _freeBlocks;
        private int _freeInodes;
        private int _blocksCount;
        private static int _blockSize = 128000;
        public static int _inodeSize { get; set; } = 2048;
        private readonly int _superBlockSize = 4096;
        private List<int> freeBlocksList;
        private int _currentDirectoryNode;

        private readonly string _path;

        private readonly object _fileLock = new object();

        public FileSystem(string path, int inodesCount = 50, int blocksCount = 5000)
        {
            if (_superBlockSize <= blocksCount * 5)
            {
                _superBlockSize = blocksCount * 5 + 256;
            }
            _currentDirectoryNode = 1;
            _path = path;
            _freeInodes = inodesCount;
            _freeBlocks = blocksCount;
            _inodesCount = inodesCount;
            _blocksCount = blocksCount;
            freeBlocksList= new List<int>();
            for (var i = 0; i < blocksCount; i++)
            {
                freeBlocksList.Add(_inodesCount+i+1);
            }
        }

        private void OverWriteSuperBlock()
        {
            string superBlock = null;
            superBlock += $"Inodes Count : {_inodesCount}".PadRight(31) + '\n';
            superBlock += $"Data Blocks Count : {_blocksCount}".PadRight(31) + '\n';
            superBlock += $"Free Inodes : {_freeInodes}".PadRight(31) + '\n';
            superBlock += $"Free Blocks : {_freeBlocks}".PadRight(31) + '\n';
            superBlock += $"Inode Size : {_inodeSize}".PadRight(31) + '\n';
            superBlock += $"Block Size : {_blockSize}".PadRight(31) + '\n';
            freeBlocksList.Sort();
            superBlock += $"Free Blocks : {string.Join(" ", freeBlocksList)}"+'\n';
            superBlock = superBlock.PadRight(_superBlockSize-1, '=') + '\n';
            var rootNodeInfo = Encoding.ASCII.GetBytes(superBlock);
            lock (_fileLock)
            {
                using (var sr = new FileStream(_path, FileMode.Open))
                {
                    sr.Seek(0, SeekOrigin.Begin);
                    sr.Write(rootNodeInfo, 0, rootNodeInfo.Length);
                }
            }

        }

        private INode ReadNode(int id)
        {
            var nodeInfo = new byte[_inodeSize];
            lock (_fileLock)
            {
                using (var sr = new FileStream(_path, FileMode.Open))
                {
                    sr.Seek(_superBlockSize + (id - 1) * _inodeSize, SeekOrigin.Begin);
                    sr.Read(nodeInfo, 0, _inodeSize);
                }
            }

            var str = Encoding.Default.GetString(nodeInfo);
            var inode = INode.GetNode(str);
            return inode;
        }

        private DataBlock ReadBlock(int id)
        {
            var blockInfo = new byte[_blockSize];
            lock (_fileLock)
            {
                using (var sr = new FileStream(_path, FileMode.Open))
                {
                    var offset = _superBlockSize + _inodeSize * _inodesCount + (id - 1) * _blockSize;
                    sr.Seek(offset, SeekOrigin.Begin);
                    sr.Read(blockInfo, 0, blockInfo.Length);
                }
            }

            var str = Encoding.Default.GetString(blockInfo);
            var a = str.Split('\n').ToList();
            a.Remove(a.First());
            var isFree = a.First().Contains("free");
            a.Remove(a.First());
            var b = new DataBlock(_inodesCount + id, _blockSize, string.Join("\n", a).Trim('*'), isFree);
            return b;
        }

        public void ReadSuperBlock()
        {
            var superBlock = new char[_superBlockSize];
            lock (_fileLock)
            {
                using (var sr = new StreamReader(_path))
                {
                    sr.ReadBlock(superBlock, 0, _superBlockSize);
                }
            }
            var lines = string.Join("", superBlock).Split('\n').Select(s => s.Trim(' ', '=')).Except(new[] {""}).ToList();

            try
            {
                var values = lines.Select(s =>s.Split(new[] {" : "}, StringSplitOptions.RemoveEmptyEntries)[1]).ToList();
                _inodesCount = Convert.ToInt32(values[0]);
                _blocksCount = Convert.ToInt32(values[1]);
                _freeInodes = Convert.ToInt32(values[2]);
                _freeBlocks = Convert.ToInt32(values[3]);
                _inodeSize = Convert.ToInt32(values[4]);
                _blockSize = Convert.ToInt32(values[5]);
            }
            catch (Exception e)
            {
                Console.WriteLine("Incorrect File :(" + e);
            }
        }

        private void WriteRootNode()
        {
            var name = System.Security.Principal.WindowsIdentity.GetCurrent().Name.Split('\\')[1];
            var rootNode = new INode(1, "Directory", "777", 1, name, "Users", _blockSize, DateTime.Now,
                new List<int>() {_inodesCount + 1});
            OverWriteNode(rootNode);
            _freeBlocks--;
            _freeInodes--;
            OverWriteSuperBlock();
        }

        private void OverWriteNode(INode iNode)
        {
            var rootNodeInfo = Encoding.ASCII.GetBytes(iNode.ToString());
            lock (_fileLock)
            {
                using (var sr = new FileStream(_path, FileMode.Open))
                {
                    sr.Seek(_superBlockSize + (iNode.Id - 1) * _inodeSize, SeekOrigin.Begin);
                    sr.Write(rootNodeInfo, 0, rootNodeInfo.Length);
                }
            }
        }

        private void OverWriteBlock(DataBlock dataBlock)
        {
            var blockInfo = Encoding.ASCII.GetBytes(dataBlock.ToString());
            lock (_fileLock)
            {
                using (var sr = new FileStream(_path, FileMode.Open))
                {
                    var offset = _superBlockSize + _inodeSize * _inodesCount + (dataBlock.Id - _inodesCount - 1) * _blockSize;
                    sr.Seek(offset, SeekOrigin.Begin);
                    sr.Write(blockInfo, 0, blockInfo.Length);
                }
            }
        }

        public void GenerateFreeFileSystem()
        {
            lock (_fileLock)
            {
                File.WriteAllText(_path, String.Empty);
            }

            OverWriteSuperBlock();
            WriteRootNode();
            for (var i = 1; i < _inodesCount; i++)
            {
                OverWriteNode(INode.GetEmptyNode(i + 1, 0));
            }

            var block = new DataBlock(_inodesCount + 1, _blockSize, ". | 1\n.. | 1\n", false);
            OverWriteBlock(block);
            freeBlocksList.Remove(block.Id);
            //for (var i = 1; i < _blocksCount; i++)
            //{
            //    OverWriteBlock(DataBlock.GetEmptyDatablock(i + 1 + _inodesCount, _blockSize));
            //}
        }
        
        public void GoToDirectory(string name)
        {
            var node = ReadNode(_currentDirectoryNode);

            var block = ReadBlock(node.DataBlocks.First() - _inodesCount);
            var nodeId = block.GetNodeId(name);
            if (nodeId != 0)
            {
                if (ReadNode(nodeId).Type != "Directory")
                {
                    Console.WriteLine("Directory with this Fullname not found. :(");
                    return;
                }
            }

            if (nodeId == 0)
            {
                Console.WriteLine("Directory with this Fullname not found. :(");
                return;
            }
            _currentDirectoryNode = nodeId;
        }

        private INode GetFreeNode()
        {
            INode freeNode = null;

            for (var i = 0; i < _inodesCount; i++)
            {
                var iNode = ReadNode(i + 1);
                if (iNode.DataBlocks.Count != 0) continue;
                freeNode = iNode;
                break;
            }
            return freeNode;
        }

        public void MakeDirectory(string name)
        {
            var node = ReadNode(_currentDirectoryNode);

            var block = ReadBlock(node.DataBlocks.First() - _inodesCount);
            var nodeId = block.GetNodeId(name);
            if (nodeId != 0)
            {
                if (ReadNode(nodeId).Type == "Directory")
                {
                    Console.WriteLine("Directory Already Exists");
                    return;
                }
            }
            var freeNode = GetFreeNode();

            if (freeNode == null)
            {
                Console.WriteLine("THERE IS NO FREE SPACE SRY");
                return;
            }

            for (var i = 0; i < _blocksCount; i++)
            {
                
                if (freeBlocksList.Count==0) continue;
                var freeBlock = new DataBlock(freeBlocksList.First(), _blockSize, $". | {freeNode.Id}\n.. | {node.Id}\n", false);
                freeBlocksList.Remove(freeBlock.Id);
                block.Add(freeNode.Id, name);
                freeNode.AddDataBlock(freeBlock.Id);
                freeNode.User = System.Security.Principal.WindowsIdentity.GetCurrent().Name.Split('\\')[1];
                freeNode.Type = "Directory";
                freeNode.LinkCount++;
                freeNode.Size += _blockSize;
                freeNode.TimeStamp = DateTime.Now;
                freeNode.Group = "Users";
                freeNode.Mode = "777";
                node.LinkCount++;
                _freeBlocks--;
                _freeInodes--;
                OverWriteBlock(freeBlock);
                OverWriteNode(node);
                OverWriteBlock(block);
                OverWriteNode(freeNode);
                OverWriteSuperBlock();
                return;
            }
        }

        private byte[] ReadFile(string fullname)
        {
            var output = string.Empty;
            var node = ReadNode(_currentDirectoryNode);

            var block = ReadBlock(node.DataBlocks.First() - _inodesCount);
            string fileName;
            string fileExtention;
            try
            {
                fileName = fullname.Split('.')[0];
                fileExtention = '.' + fullname.Split('.')[1];
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return new byte[] { };
            }

            var nodeId = block.GetNodeId(fileName);
            if (nodeId != 0)
            {
                var curNode = ReadNode(nodeId);
                if (curNode.Type == fileExtention)
                {
                    foreach (var dataBlockId in curNode.DataBlocks)
                    {
                        var newdata = ReadBlock(dataBlockId - _inodesCount).GetData();
                        output += newdata;
                    }
                }
            }
            var splited = output.Split(' ').ToList();

            var bytes = new List<byte>();
            foreach (var item in splited)
            {
                if (item.Length != 0)
                    bytes.Add(Convert.ToByte(item));
            }
            return bytes.ToArray();
        }

        public void AddTxtFile(string name, string data)
        {
            var bytes = Encoding.Default.GetBytes(data);
            AddFile(name, bytes);
        }

        public void AddFile(string fullname, byte[] bytes)
        {
            string fileName;
            string fileExtention;
            try
            {
                fileName = fullname.Split('.')[0];
                fileExtention = '.' + fullname.Split('.')[1];
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return;
            }

            var str = string.Join(" ", bytes);

            var infoBlocks = SplitByLength(str, _blockSize - 36).ToList();

            //var freeBlocks = new List<DataBlock>();
            var freeNode = GetFreeNode();

            //for (var i = 0; i < _blocksCount; i++)
            //{
            //    var block = ReadBlock(i + 1);
            //    if (block.IsFree)
            //    {
            //        freeBlocks.Add(block);
            //    }
            //}

            if (freeBlocksList.Count >= infoBlocks.Count && freeNode!=null)
            {
                _freeInodes--;
                
                var node = ReadNode(_currentDirectoryNode);
                var block = ReadBlock(node.DataBlocks.First() - _inodesCount);
                var datasize = 0;

                block.Add(freeNode.Id, fileName);
                OverWriteBlock(block);
                
                for (var i = 0; i < infoBlocks.Count; i++)
                {
                    datasize += infoBlocks[i].Length;
                    var bl = new DataBlock(freeBlocksList[i], _blockSize, infoBlocks[i], false);
                    freeBlocksList.Remove(bl.Id);
                    _freeBlocks--;
                    OverWriteBlock(bl);
                    freeNode.AddDataBlock(bl.Id);
                }

                freeNode.User = System.Security.Principal.WindowsIdentity.GetCurrent().Name.Split('\\')[1];
                freeNode.Type = fileExtention;
                freeNode.LinkCount++;
                freeNode.Size = datasize;
                freeNode.TimeStamp = DateTime.Now;
                freeNode.Group = "Users";
                freeNode.Mode = "777";
                OverWriteNode(freeNode);
                OverWriteSuperBlock();
            }
            else
            {
                Console.WriteLine("Not Enough Space");
            }
        }

        private static IEnumerable<string> SplitByLength(string str, int maxLength)
        {
            for (var index = 0; index < str.Length; index += maxLength)
            {
                yield return str.Substring(index, Math.Min(maxLength, str.Length - index));
            }
        }

        public void RemoveDirectory(string name)
        {
            var node = ReadNode(_currentDirectoryNode);
            var block = ReadBlock(node.DataBlocks.First() - _inodesCount);
            var nodeId = block.GetNodeId(name);
            if (nodeId != 0)
            {
                if (ReadNode(nodeId).Type == "Directory")
                {
                    block.Remove(name, nodeId);
                    OverWriteBlock(block);
                    ClearInode(nodeId);
                }
                else
                {
                    Console.WriteLine("Directory not found. ");
                }
            }
            else
            {
                Console.WriteLine("Directory not found. :(");
            }
        }

        public void RemoveFile(string fullName)
        {
            var node = ReadNode(_currentDirectoryNode);
            var fileName = fullName.Split('.')[0];
            var fileExtention = '.' + fullName.Split('.')[1];
            var block = ReadBlock(node.DataBlocks.First() - _inodesCount);

            var nodeId = block.GetNodeId(fileName);
            if (nodeId != 0)
            {
                if (ReadNode(nodeId).Type.Trim() == fileExtention.Trim())
                {
                    block.Remove(fileName, nodeId);
                    OverWriteBlock(block);
                    ClearInode(nodeId);
                }
                else
                {
                    Console.WriteLine("File not found. ");
                }
            }
            else
            {
                Console.WriteLine("File not found. :(");
            }
        }


        private void ClearInode(int nodeId)
        {
            var node = ReadNode(nodeId);
            foreach (var item in node.DataBlocks)
            {
                var block = ReadBlock(item - _inodesCount);

                var directoryInfo = block.GetDirectoryInfo();

                var freeBlock = new DataBlock(block.Id, _blockSize, "");

                _freeBlocks++;
                OverWriteBlock(freeBlock);
                freeBlocksList.Add(freeBlock.Id);
                
                foreach (var info in directoryInfo)
                {
                    if (info.Key.Contains('.'))
                        continue;

                    ClearInode(info.Value);

                    var freeNode = INode.GetEmptyNode(info.Value, 0);

                    _freeInodes++;
                    OverWriteNode(freeNode);
                }
            }

            node = INode.GetEmptyNode(node.Id, 0);
            _freeInodes++;
            OverWriteNode(node);
            OverWriteSuperBlock();
        }

        public void ShowDirectoryInfo()
        {
            var node = ReadNode(_currentDirectoryNode);

            var block = ReadBlock(node.DataBlocks.First() - _inodesCount);
            var directories = block.GetDirectoryInfo();

            foreach (var item in directories)
            {
                // if(item.Key.Contains('.'))
                //   continue;
                var output = String.Empty;
                var curNode = ReadNode(item.Value);
                output += curNode.Mode + "  ";
                output += curNode.LinkCount;
                output = output.PadRight(10);
                output += curNode.User;
                output = output.PadRight(20);
                output += curNode.Group;
                output = output.PadRight(30);
                output += curNode.Size;
                output = output.PadRight(40);
                output += curNode.TimeStamp + "  ";
                output += item.Key;
                if (curNode.Type != "Directory")
                    output = output.TrimEnd() + curNode.Type.Trim();
                Console.WriteLine(output);
            }
        }

        public void GoToPreviousDirectory()
        {
            var node = ReadNode(_currentDirectoryNode);
            var block = ReadBlock(node.DataBlocks.First() - _inodesCount);

            foreach (var item in block.GetDirectoryInfo())
            {
                if (item.Key.Trim() == "..")
                {
                    _currentDirectoryNode = item.Value;
                }
            }
        }

        public void SaveAs(string newPath,string name)
        {
            var desctopPath =newPath + '\\' + name;
            File.WriteAllBytes(desctopPath, ReadFile(name));
        }

        public void SaveToPc(string name)
        {
            var desctopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + '\\' +
                                 "Tanenbaum_4_48" + '\\' + name;
            File.WriteAllBytes(desctopPath, ReadFile(name));
        }

        public string GetFileInfo(string name)
        {
            var a = ReadFile(name);
            var str = Encoding.Default.GetString(a.ToArray());
            return str;
        }
    }
}

