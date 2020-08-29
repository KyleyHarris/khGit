using System;
using System.Collections.Generic;

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
            var s = GitCommands.GetAllBranches();
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

                    // any command the user types --dry will not perform the 
                    // operation but it will show the user the execution 
                    // cmd script that would run
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
            return new CmdLine(cmdLine);
        }

        private void RenderConsole()
        {
            // treating this like a console application
            //render.Clear();

            // make sure the user has the correct commit login shown
            render.WriteLn("khGit Interface - " + GitCommands.GetUserDetails());
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
                PrintCommand("S", "tash List, or ([NEW] Create Stash [K]eep Code)  "); NewLine();
                //PrintCommand("S", "[NEW] Create Stash [K]eep Code");
                PrintCommand("F", "eature Start [<name>]");
                PrintCommand("B", "ug Start");
                PrintCommand("P", "rune remote references");
                PrintCommand("G", "it (run git cmd)");
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

            // list all branches with a 1 based notation
            // master is not used by the flow at his level
            // so we disable it for this purpose
            // active branch displayed in red
            // feature/branch truncated to f/branch
            // help side listed
            for (var i = 1; i <= git.Branches.Count; i++)
            {
                var branch = git.Branches[i - 1];
                if (branch.Branch == "master")
                {
                    //                    s = s + "(locked)";
                    //                  c = branch.IsActive ? ConsoleColor.Red : ConsoleColor.Cyan;
                    continue;
                }
                render.Write($"{i.ToString().PadLeft(3)}) ", ConsoleColor.Yellow);
                ConsoleColor c = branch.IsActive ? ConsoleColor.Red : ConsoleColor.White;
                var s = branch.ToString();

                if (git.IsBranchRemoteOnly(branch.Branch))
                {
                    render.Write("origin/", ConsoleColor.Cyan);
                }
                else
                if (git.IsBranchLocalOnly(branch.Branch))
                {
                    render.Write("local/", ConsoleColor.Cyan);
                }
                if (branch.IsFeatureBranch)
                {
                    render.Write("feature/", ConsoleColor.Green);
                    s = s.Replace("feature/", "");
                    c = branch.IsActive ? ConsoleColor.Red : ConsoleColor.Yellow;
                }

                render.Write(s.PadRight(40), c);

                NewLine();



            }
            render.WriteLn("------------------------------------------------------------------", ConsoleColor.Green);
            render.WriteLn("Branch Params: [S]stash/Switch, [SP}stash/switch/pop, [SA]stash/switch/apply, [FF]Delete Local/Origin ");
            render.WriteLn("------------------------------------------------------------------", ConsoleColor.Green);

        }


        //used to pause the rendering engine to display information and wait for information
        private void Inform(string s, ConsoleColor c = ConsoleColor.White)
        {
            if (s != "")
            {
                render.Write(">> ");
                render.WriteLn(s, ConsoleColor.Green);
            }
            render.Write("Press any key to continue) ", ConsoleColor.Yellow);
            Console.ReadKey();
        }
        private void Warn(string s)
        {
            Inform(s, ConsoleColor.Red);
        }

        //this is the main command processing loop
        private void ProcessCommands(CmdLine cmdLine)
        {
            var cmd = cmdLine.FirstCommand;
            GitBranch branch;
            Int32 branchNum = cmdLine.FirstNumber;
            if (cmd.ToLower() == "git")
            {
                ExecuteShell.RunCmdProcess(cmdLine.FullCommand, false);
                return;
            }

            // if the primary command is a number then we are performing an action
            // based on a branch. any invalid numbers will be ignored
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
                        //this will only occurr if there is no develop branch.
                        CreateDevelop();
                        break;
                    case "/?":
                    case "--help":
                        //no help yet as it is built into the view for simple reminder use
                        ShowHelp();
                        break;
                    case "exit":
                    case "":
                        CloseApp = true;
                        break;
                    case "s":
                        //run the stash menu subloop for apply/delete/pop stash
                        ShowStashMenu(cmdLine);
                        break;
                    case "p":
                        Inform(GitCommands.PruneRemotes());
                        break;
                    case "f":
                        var featureName = "";
                        // a feature can be run in the form of :
                        // f this is a feature
                        // will will run a new feature called this-is-a-feature
                        if (cmdLine.Count > 1)
                        {
                            featureName = String.Join("-", cmdLine.commands, 1, cmdLine.Count - 1);
                        }
                        //if feature name is empty then we will request a name
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
            // if we have a feature name this process will run immediately with no imput
            if (featureName != "")
            {
                if (Confirm($"Create the feature branch \"{featureName}\""))
                {
                    Inform(GitCommands.CreateFeatureBranch(featureName));
                }
                return;
            }

            render.WriteLn("Feature branches follow the format feature/<name> without spaces in the name", ConsoleColor.Yellow);
            render.WriteLn("if you type \"Feature branch\" then it will become \"feature/feature-branch\"", ConsoleColor.Yellow);
            string branchName;
            branchName = GetDataFromUser("Enter the <name> or press <Enter> to cancel").Trim().Replace(' ', '-');
            if (branchName.Trim() != "")
            {
                //this flow always pushes branches to remote
                render.WriteLn("Please be patient as we create local/remote branches", ConsoleColor.Yellow);
                Inform(GitCommands.CreateFeatureBranch(branchName));
            }
        }

        //this flow is designed to work with repositories that are running pull-requests
        //on the develop branch. as such we do not run a git-flow feature finish locally
        // as it wont be accepted into the server workflow.
        // the developer will do a pull request, and when it is completed on the server
        // do a cleanup of the local branch, removing remote tracking and dropping the 
        // branches
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
                    Inform(GitCommands.DeleteFeatureBranch(branch.Branch, git.ActiveBranch.Branch.ToLower() != "develop"));
                }

            }
        }

        //sub menu for working with existing stash, or creating stash off a branch
        private void ShowStashMenu(CmdLine cmd)
        {
            //this means that the user is asking for a new named stash to be created
            if (cmd.HasArg("new"))
            {
                CreateNewStash(cmd.HasArg("K"));
                return;
            }

            // start a sub loop rendering to the console for stash code.
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
                    //skip out of the submenu
                    bDone = true;
                }
                else
                //this code is both testing and getting a stash for use further down the loop
                if (stashNum == -1 || !git.GetStash(stashNum, out stash))
                {
                    render.WriteLn(">> Invalid stash number", ConsoleColor.Red);
                }
                else
                {

                    if (cmdline.HasArg("del"))
                    {
                        //delete the selected stash
                        Inform(GitCommands.DropStash(stash.Number));
                        LoadStashes();
                    }
                    else
                    {
                        // only unstash if the stash matches the active branch
                        // or (F)orce command applied
                        // or confirmation
                        var bContinue = git.ActiveBranch.Branch == stash.Branch ||
                             cmdline.HasArg("F") ||
                             Confirm($"Unstash ({stash.Number.ToString()}) from {stash.Branch} into {git.ActiveBranch}");
                        if (bContinue)
                        {
                            Inform(GitCommands.ApplyStash(stash.Number, cmdline.HasArg("d")));
                            //refresh the stashlist before refreshing the rendering loop
                            LoadStashes();
                        }

                    }
                }
            }
        }

        //build a stash with a name for the user
        //if keepCode is true then we do not remove the stash files from the local
        //repository. this is purely a backup
        private void CreateNewStash(bool keepCode)
        {
            var s = GetDataFromUser("Type in a stash identity or <Enter> to cancel");
            if (s != "")
            {
                Inform(GitCommands.CreateStash(s, keepCode));
            }
        }

        //cmdSwitch has some options in relation to switching branches
        //   s perform stash backup before moving to a new branch so you can recover the work
        //     
        //   sp stash all work and switch to a new branch and pop it
        //      ideal if you are accidentally coding in the wrong branch and haven't committed
        //   sa stash and apply to new branch, but leave the stash available
        //   ff if the branch is a feature, then finish and close up the feature branch
        //      only do this if the pullrequest and remote doesnt exist
        private void ProcessBranchCommand(GitBranch gitBranch, CmdLine cmdSwitch)
        {
            if (gitBranch.Branch.ToLower().Equals("master"))
            {
                Inform("Switching to master branch is not supported in this tool");
                return;
            }
            if (cmdSwitch.HasArg("FF") || cmdSwitch.HasArg("FFF"))
            {
                DeleteFeatureBranch(gitBranch);

                if (cmdSwitch.HasArg("FFF"))
                {
                    DeleteOriginFeatureBranch(gitBranch);

                }
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

        private void DeleteOriginFeatureBranch(GitBranch branch)
        {

            if (!branch.IsFeatureBranch)
            {
                Inform("please select a feature branch to complete a feature", ConsoleColor.Red);
            }
            else
            {
                if (Confirm($"Permanently Delete REMOTE ORIGIN Branch: {branch.Branch}"))
                {
                    Inform(GitCommands.DeleteRemoteFeatureBranch(branch.Branch));
                }

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

