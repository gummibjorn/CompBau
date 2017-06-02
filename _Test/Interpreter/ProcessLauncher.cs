using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using RappiSharp.Compiler.Generator;

namespace _Test.Generator
{
    internal class ProcessLauncher
    {
        public static string RunVm(RappiGenerator generator, List<string> input)
        {
            var count = 2;
            if (input.Count == 0)
            {
                count = 3;
            }
            var filename = new StackTrace().GetFrame(count).GetMethod().Name + ".il";
            var folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\Generator\");

            string path = Path.Combine(folder, filename);

            try
            {
                File.Delete(path);
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Delete failed: {e}");
            }
      
            generator.Metadata.Save(path);

            var p = new Process
            {
                StartInfo =
                {
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    RedirectStandardInput = true,
                    FileName = Path.Combine(folder, @"rvm.exe"),
                    Arguments = '"' + path + '"'
                }
            };

            p.Start();
            foreach (var inp in input)
            {
                p.StandardInput.WriteLine(inp);
            }
            var output = p.StandardOutput.ReadToEnd();
            var error = p.StandardError.ReadToEnd();
            p.WaitForExit();
            return output + error;
        }
    }
}
