using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace Tanenbaum_4_48
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            var path = @"C:\Users\nnuda\source\HomeWorks_sem4\Tanenbaum_4_48\Tanenbaum_4_48\in.txt";
            var fileSystem = new FileSystem(path);

            fileSystem.GenerateFreeFileSystem();
            fileSystem.ReadSuperBlock();

            var command = string.Empty;

            while (command!="0")
            {
                command = Console.ReadLine();
                if (command == null) continue;
                var splitedCommand = command.Split(' ');

                if (command.Contains("./"))
                {
                    var name = command.Replace("./", "");
                    fileSystem.SaveToPc(name);
                    name = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + '\\' + "Tanenbaum_4_48" + '\\' +name;
                    var pi = new ProcessStartInfo(name) {Arguments = Path.GetFileName(name)};

                    if (name.Contains(".exe"))
                    {
                        Process.Start(name);
                    }
                    else
                    {
                        pi.UseShellExecute = true;
                        pi.Verb = "OPEN";
                        pi.WorkingDirectory = Path.GetDirectoryName(name) ?? throw new InvalidOperationException();

                        Process.Start(pi);
                    }
                    Console.WriteLine(fileSystem.GetFileInfo(command));
                }
                try
                {
                    switch (splitedCommand[0])
                    {
                        case "saveAs":
                            var dialog = new FolderBrowserDialog();
                            
                            if (dialog.ShowDialog() == DialogResult.OK)
                            {
                                var newPath = dialog.SelectedPath;
                                fileSystem.SaveAs(newPath, splitedCommand[1]);
                            }

                            break;

                        case "open":
                            var openFileDialog = new OpenFileDialog();

                            openFileDialog.InitialDirectory = "c:\\";
                            openFileDialog.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
                            openFileDialog.FilterIndex = 2;
                            openFileDialog.RestoreDirectory = true;

                            if (openFileDialog.ShowDialog() == DialogResult.OK)
                            {
                                try
                                {
                                    var bytes =File.ReadAllBytes(openFileDialog.FileName);
                                    fileSystem.AddFile(openFileDialog.SafeFileName, bytes);
                                }
                                catch (Exception ex)
                                {
                                    MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message);
                                }
                            }

                            break;
                        case "reset":
                            fileSystem.GenerateFreeFileSystem();
                            fileSystem.ReadSuperBlock();
                            break;
                        case "ls":
                            fileSystem.ShowDirectoryInfo();
                            break;
                        case "cd":
                            if (splitedCommand[1].Trim() == "..")
                                fileSystem.GoToPreviousDirectory();
                            else
                                fileSystem.GoToDirectory(splitedCommand[1]);
                            break;
                        case "mk":
                            fileSystem.AddFile(splitedCommand[1],new byte[]{});
                            break;
                        case "mkdir":
                            fileSystem.MakeDirectory(splitedCommand[1]);
                            break;
                        case "rmdir":
                            fileSystem.RemoveDirectory(splitedCommand[1]);
                            break;
                        case "save":
                            fileSystem.SaveToPc(splitedCommand[1]);
                            break;
                        case "touch":
                            Console.WriteLine("input Text");
                            var text = Console.ReadLine();
                            fileSystem.AddTxtFile(splitedCommand[1], text);
                            break;
                        case "rm":
                            fileSystem.RemoveFile(splitedCommand[1]);
                            break;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            //fileSystem.LoadFileSystemFromFile(@"C:\Users\nnuda\Downloads\Metallica - Discography\Albums\1991 - Metallica\1991 Germany Vertigo 510 022-2 LE\Metallica - Metallica (Vertigo 510 022-2).flac");//@"C:\Users\nnuda\Downloads\Metallica - Discography\Albums\1991 - Metallica\1991 Germany Vertigo 510 022-2 LE\Metallica - Metallica (Vertigo 510 022-2).flac"//fileSystem.OverWriteNode(fileSystem.ReadNode(1));
            //fileSystem.GoToDirectory("name");
            //fileSystem.MakeDirectory("name");
            //fileSystem.GoToDirectory("name");
            //fileSystem.MakeDirectory("name1");
            //fileSystem.ShowDirectoryInfo();
            //fileSystem.GoToPreviousDirectory();
            //fileSystem.ShowDirectoryInfo();
            //Thread.Sleep(2000);
            //fileSystem.MakeDirectory("name");
            //fileSystem.ShowDirectoryInfo();
            //fileSystem.GoToDirectory("name");
            //fileSystem.MakeDirectory("name");
            //fileSystem.ShowDirectoryInfo();
            //fileSystem.AddTxtFile("test", "В 1955 году была сформирована группа пользователей научных приложений для компьютеров IBM 701. Группа эта работала на добровольных началах, и получила название SHARE. Со временем эта организация стала развиваться, и в конце концов превратилась в форум, где обсуждались различные технические вопросы, касающиеся языков программирования, операционных систем, баз данных и др. Но главным их ресурсом, несомненно, была библиотека исходного кода. Первоначально IBM выпускала свои операционные системы с открытым исходным кодом, и многие системные программисты модифицировали существующие функции или добавляли новые, а затем делились своими знаниями с сообществом. Кроме того, библиотека SHARE содержала куски кода, пригодные для повторного использования — реализации алгоритмов сортировки, различных математических функций, а также инструменты программирования, такие как отладчики. Тот, кто писал программу, в которой требовалась реализация некоего общего алгоритма, вначале пытался найти ее в библиотеке SHARE. \r\nКачество кода, который содержался в библиотеке, было достаточно высоким, хотя никто э");
            //Console.WriteLine(fileSystem.ReadFile("test.txt"));

            //fileSystem.RemoveFile("test.txt");
            //fileSystem.SaveToPc("test.txt");
            //fileSystem.ShowDirectoryInfo();
            //Console.WriteLine(fileSystem.ReadNode(1));



            //fileSystem.ReadBlock(2);

            //Console.WriteLine(a);
            //Console.ReadKey();
        }
    }
}
