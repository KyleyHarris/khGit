using System;

namespace khGit
{
    class ConsoleManager
    {
        private ConsoleRender render = new ConsoleRender();
        private GitContainer git = new GitContainer();
        public ConsoleManager()
        {
        }

        private bool LoadBranches()
        {
            var s = GitCommands.GetBranchList();
            if (!s.Contains("fatal:"))
            {
                git.ParseBranch(s);
                return git.Branches.Count > 0;
            }
            else
            {
                return false;
            }
        }
        private bool IsGITRepo
        {
            get
            {
                return LoadBranches();
            }
        }
        internal void Run()
        {

            if (!IsGITRepo)
            {
                Warn("There is no git repository at the current location. Exiting....");
                return;
            }

            ProcessInputCommands(); // main console processing loop

        }
        private bool CloseApp { get; set; }

        private void ProcessInputCommands()
        {
            while (!CloseApp)
            {
                RenderConsole();
                var data = GetCommandFromUser();
                try
                {

                    ExecuteShell.DryRun = data.HasArg("--dry");
                    try
                    {
                        ProcessCommands(data);
                    }
                    finally
                    {
                        ExecuteShell.DryRun = false;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }

        private bool Confirm(string msg)
        {
            render.Write(msg + " (Y/N)?", ConsoleColor.Green);
            var confirmed = Console.ReadKey().Key == ConsoleKey.Y;
            render.WriteLn();
            return confirmed;
        }
        private string GetDataFromUser(string msg)
        {
            render.Write(msg + ") ", ConsoleColor.Green);
            return Console.ReadLine();
        }
        private CmdLine GetCommandFromUser()
        {
            render.Write("Pick a command to run, <Enter> to quit) ", ConsoleColor.Green);
            var cmdLine = Console.ReadLine();
            return new CmdLine(cmdLine.Split(new char[] { ' ' }));
        }

        private void RenderConsole()
        {
            render.Clear();

            render.WriteLn("khGit Interface - "+GitCommands.GetUserDetails());

            RenderBranches();
            LoadStashes();
            render.WriteLn();
            if (!git.Branches.Exists("develop"))
            {
                PrintCommand("I", "create develop branch");
                NewLine();
            }
            else
            {
                if (git.Stashes.Count > 0)
                {
                    render.WriteLn("-----GIT Repo has stashes available to use-----------", ConsoleColor.Green);
                }
                PrintCommand("S", "tash List, or ([NEW] Create Stash [K]eep Code)  ");NewLine();
                //PrintCommand("S", "[NEW] Create Stash [K]eep Code");
                PrintCommand("F", "eature Start [<name>]");
                PrintCommand("B", "ug Start");
                //build normal command structure
            }
            NewLine(); NewLine();
        }

        private void LoadStashes()
        {
            git.ParseStashes(GitCommands.GetStashList());
        }

        private void RenderStashes()
        {
            if (git.Stashes.Count > 0)
            {
                render.WriteLn("-----Stashes-----------", ConsoleColor.Green);

                foreach (GitStash stash in git.Stashes)
                {
                    render.Write($"{stash.Number.ToString().PadLeft(3)}) ", ConsoleColor.Yellow);
                    ConsoleColor c = stash.Branch == git.ActiveBranch.Branch ? ConsoleColor.Red : ConsoleColor.White;
                    render.Write(stash.Branch, c);
                    render.WriteLn("-> " + stash.Note, ConsoleColor.Yellow);
                }
                render.WriteLn("-----------------------", ConsoleColor.Green);
                render.Write("Enter Stash number to pop into Active Branch -> ", ConsoleColor.Green);
                render.WriteLn(git.ActiveBranch.Branch, ConsoleColor.Red);
                render.WriteLn();
                render.WriteLn("Arguments Can Be Combined");
                render.WriteLn("<StashNum> [D][F][DEL]", ConsoleColor.Yellow);
                render.WriteLn("D   -> will delete the stash after applying to the active branch");
                render.WriteLn("F   -> will force the stash into active branch if the branches don't match");
                render.WriteLn("DEL -> just delete the stash, no action");
            }
        }
        private int commandIndex = 0;
        private readonly int commandsPerLine = 2;
        private void NewLine()
        {
            render.WriteLn();
            commandIndex = 0;
        }

        private void PrintCommand(string cmd, string msg)
        {
            var mnu = cmd + ")";
            render.Write(mnu.PadLeft(3), ConsoleColor.Yellow);
            render.Write(msg.PadRight(50));
            commandIndex++;
            if (commandIndex == commandsPerLine)
            {
                NewLine();
            }
        }

        // primary console rendering of the Branch List and control instructions for the main screen
        private void RenderBranches()
        {
            LoadBranches();
            render.WriteLn("-Type Branch number to checkout-----------------------------------", ConsoleColor.Green);
            var firstY = Console.CursorTop;
            for (var i = 1; i <= git.Branches.Count; i++)
            {
                var branch = git.Branches[i - 1];
                render.Write($"{i.ToString().PadLeft(3)}) ", ConsoleColor.Yellow);
                ConsoleColor c = branch.IsActive ? ConsoleColor.Red : ConsoleColor.White;
                var s = branch.ToString();
                if (branch.Branch == "master")
                {
                    s = s + "(locked)";
                    c = ConsoleColor.Cyan;
                }
                else
                    if (branch.IsFeatureBranch)
                {
                    s = s.Replace("feature/", "  f/");
                    c = ConsoleColor.Yellow;
                }
                render.WriteLn(s, c);
            }
            var x = Console.CursorLeft;
            var y = Console.CursorTop;
            render.MarginX = 46;
            render.MoveXY(46, firstY);
            render.WriteLn("<Branch-Num> [S][SP][SA]", ConsoleColor.Yellow);
            render.WriteLn("S -> will create a stash before switching");
            render.WriteLn("SP-> will create a stash and apply the stash ");
            render.WriteLn("     in the new branch and delete the stash");
            render.WriteLn("SA-> will create a stash and apply the stash");
            render.WriteLn("     in the new branch and preserve the stash");
            render.WriteLn("FF-> Permanently delete the specified feature ", ConsoleColor.Magenta);
            render.WriteLn("     branch from the local git", ConsoleColor.Magenta);

            Console.CursorTop = Math.Max(firstY + git.Branches.Count, Console.CursorTop);
            render.MarginX = 0;
            render.MoveXY(0, Console.CursorTop);
            render.WriteLn("------------------------------------------------------------------", ConsoleColor.Green);
            render.WriteLn();
        }

        private void Inform(string s, ConsoleColor c = ConsoleColor.White)
        {
            render.Write(">> ");
            render.WriteLn(s, ConsoleColor.Red);
            render.Write("Press <Enter> to continue) ");
            Console.ReadLine();
        }
        private void Warn(string s)
        {
            Inform(s, ConsoleColor.Red);
        }
        private void ProcessCommands(CmdLine cmdLine)
        {
            var cmd = cmdLine.FirstCommand;
            GitBranch branch;
            Int32 branchNum = cmdLine.FirstNumber;
            if (branchNum != -1)
            {
                if (git.ValidBranch(branchNum, out branch))
                {
                    ProcessBranchCommand(branch, cmdLine);
                }
                else
                {
                    Warn($"{branchNum} does not correspond to a branch");
                }
            }
            else
            {

                switch (cmd)
                {
                    case "i":
                        CreateDevelop();
                        break;
                    case "/?":
                    case "--help":
                        ShowHelp();
                        break;
                    case "exit":
                    case "":
                        CloseApp = true;
                        break;
                    case "s":
                        ShowStashMenu(cmdLine);
                        break;
                    case "f":
                        var featureName = "";
                        if (cmdLine.Count > 1)
                        {
                            featureName = String.Join("-", cmdLine.commands, 1, cmdLine.Count - 1);
                        }
                        CreateFeatureBranch(featureName);
                        break;
                    default:
                        Console.WriteLine("Unknown Command");
                        break;
                }
            }
        }

        private void CreateFeatureBranch(string featureName)
        {

            render.WriteLn("Feature branches follow the format feature/<name> without spaces in the name", ConsoleColor.Yellow);
            render.WriteLn("if you type \"Feature branch\" then it will become \"feature/feature-branch\"", ConsoleColor.Yellow);
            if (featureName != "")
            {
                if (Confirm($"Create the feature branch \"{featureName}\""))
                {
                    Inform(GitCommands.CreateFeatureBranch(featureName));
                }
                return;
            }

            string branchName;
            branchName = GetDataFromUser("Enter the <name> or press <Enter> to cancel").Trim().Replace(' ', '-');
            if (branchName.Trim() != "")
            {
                render.WriteLn("Please be patient as we create local/remote branches", ConsoleColor.Yellow);
                Inform(GitCommands.CreateFeatureBranch(branchName));
            }
        }
        private void DeleteFeatureBranch(GitBranch branch)
        {

            if (!branch.IsFeatureBranch)
            {
                Inform("please select a feature branch to complete a feature", ConsoleColor.Red);
            }
            else
            {
                if (Confirm($"Permanently Delete Branch: {branch.Branch}"))
                {
                    Inform(GitCommands.DeleteFeatureBranch(branch.Branch));
                }

            }



        }

        private void ShowStashMenu(CmdLine cmd)
        {
            if (cmd.HasArg("new"))
            {
                CreateNewStash(cmd.HasArg("K"));
                return;
            }
            bool bDone = false;
            Int32 stashNum;
            while (!bDone && git.Stashes.Count > 0)
            {
                render.Clear();
                RenderStashes();
                var cmdline = GetCommandFromUser();
                stashNum = cmdline.FirstNumber;
                GitStash stash;
                if (!cmdline.Valid)
                {
                    bDone = true;
                }
                else
                if (stashNum == -1 || !git.GetStash(stashNum, out stash))
                {
                    render.WriteLn(">> Invalid stash number", ConsoleColor.Red);
                }
                else
                {
                    if (cmdline.HasArg("del"))
                    {
                        Inform(GitCommands.DropStash(stash.Number));
                        LoadStashes();
                    }
                    else
                    {
                        var bContinue = git.ActiveBranch.Branch == stash.Branch ||
                             cmdline.HasArg("F") ||
                             Confirm($"Unstash ({stash.Number.ToString()}) from {stash.Branch} into {git.ActiveBranch}");
                        if (bContinue)
                        {
                            Inform(GitCommands.ApplyStash(stash.Number, cmdline.HasArg("d")));
                            LoadStashes();
                        }

                    }
                }
            }
        }

        private void CreateNewStash(bool keepCode)
        {
            var s = GetDataFromUser("Type in a stash identity or <Enter> to cancel");
            if (s != "")
            {
                Inform(GitCommands.CreateStash(s, keepCode));
            }
        }

        //cmdSwitch has some options in relation to switching branches
        //   -s (perform stash push/pop)
        //   -ss (perform named stash) 
        private void ProcessBranchCommand(GitBranch gitBranch, CmdLine cmdSwitch)
        {
            if (gitBranch.Branch.ToLower().Equals("master"))
            {
                Inform("Switching to master branch is not supported in this tool");
                return;
            }
            if (cmdSwitch.HasArg("FF"))
            {
                DeleteFeatureBranch(gitBranch);
                return;
            }
            if (!gitBranch.IsActive)
            {
                //                render.WriteLn("S -> will create a stash before switching");
                //               render.WriteLn("SP-> will create a stash and apply the stash in the new branch and delete the stash");
                //              render.WriteLn("SA-> will create a stash and apply the stash in the new branch and preserve the stash");

                StashKind stashKind = StashKind.None;

                if (cmdSwitch.HasArg("S"))
                {
                    stashKind = StashKind.Stash;
                }
                else
                if (cmdSwitch.HasArg("SP"))
                {
                    stashKind = StashKind.StashAndPop;
                }
                else
                if (cmdSwitch.HasArg("SA"))
                {
                    stashKind = StashKind.StashAndApply;
                }

                Inform(GitCommands.CheckoutBranch(gitBranch.Branch, stashKind));
            }
        }

        private void CreateDevelop()
        {
            if (!git.Branches.Exists("develop"))
            {
                Inform(GitCommands.CreateDevelopBranch());
            }
        }

        private void ShowHelp()
        {
            throw new NotImplementedException();
        }
    }
}

