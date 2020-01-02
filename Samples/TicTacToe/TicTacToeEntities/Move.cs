using System;
using System.Collections.Generic;
using System.Text;

namespace TicTacToeEntities
{
    [Serializable]
    public class Move
    {
        public int X { get; set; }
        public int Y { get; set; }
    }
}