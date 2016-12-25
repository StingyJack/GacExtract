using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            var process = new Process();
            //your path may vary
            process.StartInfo.FileName = @"C:\Program Files (x86)\Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.6.1 Tools\gacutil.exe";
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.Arguments = "/l";
            process.Start();

            var consoleOutput = process.StandardOutput;


            var assemblyList = new List<string>();
            var startAdding = false;
            while (consoleOutput.EndOfStream == false)
            {
                var line = consoleOutput.ReadLine();
                if (line.IndexOf("The Global Assembly Cache contains the following assemblies", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    startAdding = true;
                    continue;
                }

                if (startAdding == false)
                {
                    continue;
                }

                //add any other filter conditions (framework version, etc)
                if (line.IndexOf("System.", StringComparison.OrdinalIgnoreCase) < 0)
                {
                    continue;
                }

                assemblyList.Add(line.Trim());
            }

            var collectedRecords = new List<string>();
            var failedToLoad = new List<string>();

            Console.WriteLine($"Found {assemblyList.Count} assemblies");
            var currentItem = 1;


            foreach (var gacAssemblyInfo in assemblyList)
            {
                Console.SetCursorPosition(0, 2);
                Console.WriteLine($"On {currentItem} of {assemblyList.Count} ");
                Console.SetCursorPosition(0, 3);
                Console.WriteLine($"Loading {gacAssemblyInfo}");
                currentItem++;

                try
                {
                    var asm = Assembly.Load(gacAssemblyInfo);

                    foreach (Type t in asm.GetTypes())
                    {
                        if (t.Name.EndsWith("Attribute", StringComparison.OrdinalIgnoreCase))
                        {
                            collectedRecords.Add($"{t.FullName} - {t.Assembly.FullName}");
                        }
                    }

                }
                catch (Exception ex)
                {
                    failedToLoad.Add($"FAILED to load {gacAssemblyInfo} - {ex}");
                    Console.SetCursorPosition(1, 9);
                    Console.WriteLine($"Failure to load count: {failedToLoad.Count}");
                    Console.SetCursorPosition(4, 10);
                    Console.WriteLine($"Last Fail: {gacAssemblyInfo}");
                }
            }

            var fileBase = System.IO.Path.GetRandomFileName();
            var goodFile = $"{fileBase}_good.txt";
            var failFile = $"{fileBase}_failedToLoad.txt";
            System.IO.File.WriteAllLines(goodFile, collectedRecords);
            System.IO.File.WriteAllLines(failFile, failedToLoad);
            Console.SetCursorPosition(0, 15);
            Console.WriteLine($"Matching types: {goodFile}");
            Console.WriteLine($"Failures: {failFile}");
            Console.WriteLine("Press ENTER to exit");
            Console.ReadLine();
        }
    }
}
