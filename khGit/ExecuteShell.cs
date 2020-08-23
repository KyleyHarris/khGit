using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;

namespace khGit
{
    internal class QuickSync
    {
        private readonly object balanceLock = new object();
        public bool Sync
        {
            get
            {
                lock (balanceLock)
                    return sync;
            }
            set
            {
                lock (balanceLock)
                    sync = value;
            }
        }

        private bool sync;
    }
    class ExecuteShell
    {
        public static bool DryRun = false;
        public static string GetOutput(string cmd)
        {

            if (DryRun)
            {
                return cmd;
            }
            string output;
            QuickSync sync = new QuickSync();
            sync.Sync = false;

            var thread = new Thread(delegate ()
            {
                bool writeln = false;
                Thread.Sleep(250);
                while (!sync.Sync)
                {
                    writeln = true;
                    Console.Write('.');
                    Thread.Sleep(100);
                }
                if (writeln) Console.WriteLine();
                sync.Sync = false;
            });
            thread.Start();
            try
            {
                // thanks to https://stackoverflow.com/questions/206323/how-to-execute-command-line-in-c-get-std-out-results
                //https://docs.microsoft.com/en-us/dotnet/api/system.diagnostics.process.standardoutput?redirectedfrom=MSDN&view=netcore-3.1#System_Diagnostics_Process_StandardOutput

                using (Process process = new Process())
                {
                    process.StartInfo.FileName = "cmd.exe";
                    process.StartInfo.Arguments = "/C " + cmd;
                    process.StartInfo.WorkingDirectory = Directory.GetCurrentDirectory();
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.Start();

                    // Synchronously read the standard output of the spawned process.
                    StreamReader reader = process.StandardOutput;
                    output = reader.ReadToEnd();

                    // Write the redirected output to this application's window.


                    process.WaitForExit();
                }
            }
            finally
            {
                sync.Sync = true;
                while (sync.Sync) { Thread.Sleep(25); }
            }
            return output;

        }
    }
}

