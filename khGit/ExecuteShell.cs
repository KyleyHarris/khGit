using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;

namespace khGit
{
    class ExecuteShell
    {
        public static bool DryRun = false;
        public static string RunCmdProcess(string cmd, bool redirectToResult = true)
        {

            if (DryRun)
            {
                return cmd;
            }
            string output = "";
            EventWaitHandle workComplete = new EventWaitHandle(false, EventResetMode.ManualReset);
            EventWaitHandle threadComplete = new EventWaitHandle(false, EventResetMode.ManualReset);
            var threadException = "";
            // thanks to https://stackoverflow.com/questions/206323/how-to-execute-command-line-in-c-get-std-out-results
            //https://docs.microsoft.com/en-us/dotnet/api/system.diagnostics.process.standardoutput?redirectedfrom=MSDN&view=netcore-3.1#System_Diagnostics_Process_StandardOutput
            var processExecuteThread = new Thread(
                delegate ()
                {
                    try
                    {
                        using (Process process = new Process())
                        {
                            process.StartInfo.FileName = "cmd.exe";
                            process.StartInfo.Arguments = "/C " + cmd;
                            process.StartInfo.WorkingDirectory = Directory.GetCurrentDirectory();
                            process.StartInfo.UseShellExecute = false;
                            process.StartInfo.RedirectStandardOutput = redirectToResult;
                            process.Start();

                            // Synchronously read the standard output of the spawned process.
                            if (redirectToResult)
                            {
                                StreamReader reader = process.StandardOutput;
                                output = reader.ReadToEnd();
                                //we dont need to wait for the process exit to escape out. we have our answer
                                threadComplete.Set();
                            }
                            // Write the redirected output to this application's window.


                            process.WaitForExit();
                            //we want to wait when not redirecting
                            threadComplete.Set();
                        }
                    }
                    catch (Exception e)

                    {
                        threadException = e.Message;
                        threadComplete.Set();
                    }

                });

            if (!redirectToResult)
            {
                Console.WriteLine(">" + cmd);
            }
            processExecuteThread.Start();
            bool writeln = false;
            while (!threadComplete.WaitOne(1000))
            {
                writeln = true;
                Console.WriteLine("processing....>");
            }
            if (writeln) Console.WriteLine();

            return output;

        }
    }
}

