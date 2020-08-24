using System;

namespace khGit
{
    class CmdLine
    {
        public readonly string[] commands;
        private int firstNumber;
        public string FullCommand { get; private set; }
        public CmdLine(string commands)
        {
            this.FullCommand = commands ?? throw new ArgumentNullException(nameof(commands));
            this.commands = commands.Split(new char[] { ' ' });
        }
        public bool Valid { get { return Count > 0 && commands[0].Trim() != ""; } }
        public int Count { get { return commands.Length; } }
        public bool FirstIsNumber { get { return Valid ? int.TryParse(commands[0], out firstNumber) : false; } }
        public int FirstNumber
        {
            get
            {
                if (FirstIsNumber)
                {
                    return firstNumber;
                }
                else
                {
                    return -1;
                }
            }
        }
        public string FirstCommand { get { return Valid ? commands[0].ToLower() : ""; } }
        public bool HasArg(string param)
        {
            var search = param.ToLower();

            for (var i = 0; i < Count; i++)
            {
                if (search.Equals(commands[i].ToLower()))
                {
                    return true;
                }

            }
            return false;
        }
        public int LastArgumentIndex { get { return Count - 1; } }
        public string Argument(int index)
        {
            return index < Count ? commands[index].ToLower() : "";
        }

        internal bool HasNumberArg(out int number)
        {
            for (var i = 0; i < Count; i++)
            {
                if (int.TryParse(commands[i], out number))
                {
                    return true;
                }

            }
            number = -1;
            return false;
        }
    }
}
