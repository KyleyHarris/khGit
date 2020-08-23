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
        public void AddDistinct(GitBranch b)
        {
            if (!Exists(b.Branch))
            {
                Add(b);
            }
        }
    }

    class GitBranch
    {
        public static string SanitizeName(string name)
        {
            return name.Replace('*', ' ').Trim();
        }
        public static bool IsOrigin(string name)
        {
            return SanitizeName(name).ToLower().StartsWith("origin/");
        }
        public static bool IsFeature(string name)
        {
            return SanitizeName(name).ToLower().StartsWith("feature/");
        }
        public static string CommonName(string name)
        {
            if (name.ToLower().StartsWith("origin/"))
            {
                return name.Substring(7);
            }
            else
            {
                return name;
            }
        }
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
                return IsFeature(Branch);
            }
        }

        public GitBranch(string branch, bool isActive = false)
        {
            this.Branch = branch.Trim();
            this.IsActive = isActive;
        }
    }
}
