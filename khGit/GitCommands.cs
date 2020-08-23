using System;

namespace khGit
{
    static class GitCommands
    {
        public static string GetBranchList()
        {
            return ExecuteShell.GetOutput("git branch");
        }
        public static string CreateDevelopBranch()
        {
            return ExecuteShell.GetOutput("git checkout master && git branch develop && git push -u origin develop && git checkout develop");
        }

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
            return ExecuteShell.GetOutput($"{before} git checkout {branch} {after}");
        }

        public static string GetStashList()
        {
            return ExecuteShell.GetOutput($"git stash list");
        }

        public static string ApplyStash(int number, bool dropStash)
        {
            var stash = "stash@{" + number.ToString() + "}";
            var cmd = dropStash ? "pop" : "apply";
            return ExecuteShell.GetOutput($"git stash {cmd} {stash} ");
        }

        public static string DropStash(int number)
        {
            var stash = "stash@{" + number.ToString() + "}";
            return ExecuteShell.GetOutput($"git stash drop {stash} ");
        }

        internal static string CreateStash(string s, bool keepCode)
        {
            var msg = '"' + s + '"';
            var sKeepCmd = keepCode ? " &&  git stash apply " : "";
            return ExecuteShell.GetOutput($"git stash push -u  -m {msg} {sKeepCmd} ");
        }

        public static string DeleteFeatureBranch(string branch)
        {
            return ExecuteShell.GetOutput($"git checkout develop && git branch -d {branch}");
        }

        public static string CreateFeatureBranch(string branchName)
        {
            return ExecuteShell.GetOutput($"git checkout develop &&  git checkout -b feature/{branchName} && git push -u origin feature/{branchName}  ");

        }

        public static string GetUserDetails()
        {
            return ExecuteShell.GetOutput("git config user.name && git config user.email").Replace("\n", ",");
        }
    }
}
