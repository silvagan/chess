using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace chess
{
    internal class Profile
    {
        public string name { get; set; }
        public int WinCount { get; set; }
        public int LossCount { get; set; }

        public Profile(string name, int winCount, int lossCount)
        {
            this.name = name;
            this.WinCount = winCount;
            LossCount = lossCount;
        }
    }
}
