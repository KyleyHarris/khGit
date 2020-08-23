using System.Collections.Generic;

namespace khGit
{
    class GitBranches : List<GitBranch>
    {
        public bool Exists(string branchName)
        {
            for (var i = 0; i < Count; i++)
            {
                if (this[i].Branch.Equals(branchName))
                {
                    return true;
                }
            }
            return false;
        }
    }

    class GitBranch
    {
        public override string ToString()
        {
            return Branch;
        }
        public string Branch { get; private set; }
        public bool IsActive { get; private set; }
        public bool IsFeatureBranch
        {
            get
            {
                return Branch.ToLower().StartsWith("feature/");
            }
        }

        public GitBranch(string branch, bool isActive = false)
        {
            this.Branch = branch.Trim();
            this.IsActive = isActive;
        }
    }
}
