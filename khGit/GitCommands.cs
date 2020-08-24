using System;

namespace khGit
{
    static class GitCommands
    {
        public static string GetBranchList() => ExecuteShell.RunCmdProcess("git branch");
        public static string CreateDevelopBranch() => ExecuteShell.RunCmdProcess("git checkout master && git branch develop && git push -u origin develop && git checkout develop", false);

        public static string CheckoutBranch(string branch, StashKind stashKind)
        {
            string before = "";
            string after = "";
            switch (stashKind)
            {

                case StashKind.Stash:
                    before = $"git stash push -u -m \"khGit auto-switch-stash {DateTime.Now.ToString()}\"  &&  ";
                    break;
                case StashKind.StashAndPop:
                    before = $"git stash push -u && ";
                    after = "&&  git stash pop";
                    break;
                case StashKind.StashAndApply:
                    before = $"git stash push -u -m \"khGit auto-switch-stash {DateTime.Now.ToString()}\" &&  ";
                    after = "&&  git stash apply";
                    break;
            }
            return ExecuteShell.RunCmdProcess($"{before} git checkout {branch} {after}", false);
        }

        public static string GetStashList() => ExecuteShell.RunCmdProcess($"git stash list");

        public static string ApplyStash(int number, bool dropStash)
        {
            var stash = "stash@{" + number.ToString() + "}";
            var cmd = dropStash ? "pop" : "apply";
            return ExecuteShell.RunCmdProcess($"git stash {cmd} {stash} ", false);
        }

        public static string DropStash(int number)
        {
            var stash = "stash@{" + number.ToString() + "}";
            return ExecuteShell.RunCmdProcess($"git stash drop {stash} ", false);
        }

        internal static string CreateStash(string s, bool keepCode)
        {
            var msg = '"' + s + '"';
            var sKeepCmd = keepCode ? " &&  git stash apply " : "";
            return ExecuteShell.RunCmdProcess($"git stash push -u  -m {msg} {sKeepCmd} ", false);
        }

        public static string DeleteFeatureBranch(string branch, bool switchToDevelop)
        {
            string before = "";
            if (switchToDevelop) before = "git checkout develop && ";

            return ExecuteShell.RunCmdProcess($"{before} git branch -d {branch}", false);
        }

        public static string CreateFeatureBranch(string branchName)
        {
            var s = $"git checkout develop &&  git checkout -b feature/{branchName} && git push -u origin feature/{branchName}  ";
            return ExecuteShell.RunCmdProcess(s, false);
        }

        public static string GetUserDetails() => ExecuteShell.RunCmdProcess("git config user.name && git config user.email").Replace("\n", ",");

        public static string GetAllBranches() => ExecuteShell.RunCmdProcess("git branch && git branch --list -r");
        public static string PruneRemotes() => ExecuteShell.RunCmdProcess("git remote prune origin", false);

        public static string DeleteRemoteFeatureBranch(string branch)
        {
            return ExecuteShell.RunCmdProcess($"git push origin --delete {branch}", false);
        }
    }
}
