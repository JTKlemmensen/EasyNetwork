using System;
using System.Collections.Generic;
using System.Text;

namespace TicTacToeEntities
{
    [Serializable]
    public class GameStarted
    {
        public bool YourTurn { get; set; }
    }
}