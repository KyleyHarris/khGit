using System;

namespace khGit
{
    class GitStash
    {
        public readonly int Number;
        public readonly string Branch;
        public readonly string Note;

        public GitStash(int number, string branch, string note)
        {
            Number = number;
            Branch = branch ?? throw new ArgumentNullException(nameof(branch));
            Note = note ?? throw new ArgumentNullException(nameof(note));
        }

    }
}
