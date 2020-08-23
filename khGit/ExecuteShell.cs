using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace khGit
{
    class ExecuteShell
    {
        public static bool DryRun = false;
        public static string GetOutput(string cmd)
        {
            if (DryRun)
            {
                return cmd;
            }
            // thanks to https://stackoverflow.com/questions/206323/how-to-execute-command-line-in-c-get-std-out-results
            //https://docs.microsoft.com/en-us/dotnet/api/system.diagnostics.process.standardoutput?redirectedfrom=MSDN&view=netcore-3.1#System_Diagnostics_Process_StandardOutput

            using (Process process = new Process())
            {
                process.StartInfo.FileName = "cmd.exe";
                process.StartInfo.Arguments = "/C "+cmd;
                process.StartInfo.WorkingDirectory = Directory.GetCurrentDirectory();
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.Start();

                // Synchronously read the standard output of the spawned process.
                StreamReader reader = process.StandardOutput;
                string output = reader.ReadToEnd();

                // Write the redirected output to this application's window.


                process.WaitForExit();
                return output;
            }
        }
    }
}
