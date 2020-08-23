using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace khGit
{

    class GitContainer
    {
        public readonly GitBranches Branches = new GitBranches();
        public readonly IList<GitStash> Stashes = new List<GitStash>();
        public GitBranch ActiveBranch { get; private set; }
        public bool IsFeatureBranchActive
        {
            get
            {
                return ActiveBranch.IsFeatureBranch;
            }
        }

        public void ParseBranch(string branches)
        {
            Branches.Clear();
            GitBranch gitBranch;
            ActiveBranch = null;
            var branchList = branches.Split("\n");
            for (var i = 0; i < branchList.Length; i++)
            {
                var branch = branchList[i].Replace('*', ' ');
                if (branch.Trim() == "")
                {
                    continue;
                }

                if (branch != branchList[i])
                {
                    
                    gitBranch = new GitBranch(branch, true);
                    ActiveBranch = gitBranch;
                }
                else
                {
                    gitBranch = new GitBranch(branch);
                }

                Branches.Add(gitBranch);

            }

        }
        public void ParseStashes(string data)
        {
            Stashes.Clear();
            var pattern = @"\bstash\b@\{(\d*)\}:\sOn\s(\w*):\s(.*)";
            foreach (Match match in Regex.Matches(data, pattern, RegexOptions.IgnoreCase ^ RegexOptions.Multiline))
            {
                var stash = new GitStash(int.Parse(match.Groups[1].Value), match.Groups[2].Value, match.Groups[3].Value);
                Stashes.Add(stash);

            }
        }

        public bool GetStash(int stashNum, out GitStash stash)
        {
            foreach (var _stash in Stashes)
            {
                if (_stash.Number == stashNum)
                {
                    stash = _stash;
                    return true;
                }
            }
            stash = null;
            return false;
        }

        public bool ValidBranch(int number, out GitBranch branch)
        {
            if (number > 0 && number <= Branches.Count)
            {
                branch = Branches[number - 1];
                return true;
            }
            branch = null;
            return false;
        }
    }
}
