using EasyNetwork.Network.Abstract;
using EasyNetwork.Network.Attributes;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TicTacToeEntities;

namespace TicTacToe
{
    public class ClientManager
    {
        // 0 => empty, 1 = you, 2 = opponent
        private int[][] grid = new int[3][];
        private IObjectConnection connection;

        public ClientManager()
        {
            for (int i = 0; i < 3; i++)
                grid[i] = new int[3];
        }

        [Connect]
        public void OnConnected(IObjectConnection connection)
        {
            Console.WriteLine("Connected to the server.");
            Console.WriteLine("Waiting for another player...");
            this.connection = connection;
        }

        [Command]
        public void OnGameStarted(IObjectConnection connection, GameStarted started)
        {
            if (started.YourTurn)
            {
                OutputBoard();
                Task.Run(YourTurn);
            }
            else
            {
                OutputBoard();
                Console.WriteLine("Waiting for your opponent to make his play");
            }
        }

        [Command]
        public void OnYourTurn(IObjectConnection connection, YourTurn turn)
        {
            grid[turn.X][turn.Y] = 2;
            OutputBoard();
            Task.Run(YourTurn);
        }

        [Command]
        public void OnYouLost(IObjectConnection connection, YouLost lost)
        {
            Console.Clear();
            Console.WriteLine("You lost.");
        }

        [Command]
        public void OnYouLost(IObjectConnection connection, YouWon won)
        {
            Console.Clear();
            Console.WriteLine("You won.");
        }

        private void YourTurn()
        {
            Console.WriteLine("It is your turn:");

            Move move = null;

            while (true)
            {
                string input = Console.ReadLine();
                move = GetMove(input);
                if (move != null && TileFree(move.X, move.Y))
                    break;
            }

            grid[move.X][move.Y] = 1;
            OutputBoard();
            Console.WriteLine("Waiting for your opponent to make his play");

            connection.SendObject(move);
        }

        private bool TileFree(int x, int y)
        {
            return grid[x][y] == 0;
        }

        private Move GetMove(string input)
        {
            if (input == null || input.Length != 2)
                return null;

            if (input[0] == 'A' || input[0] == 'B' || input[0] == 'C')
                if (input[1] == '0' || input[1] == '1' || input[1] == '2')
                    return new Move { X=input[0]-'A', Y=input[1]-'0' };

            return null;
        }

        private void OutputBoard()
        {
            Console.Clear();
            Console.WriteLine("How to play:");
            Console.WriteLine("Type the coordinate of where you want to place your tile.");
            Console.WriteLine("For example, if you want to place in the middle, type: B1");
            Console.WriteLine();

            for(int y=2;y>=0;y--)
                Console.WriteLine(y+"["+ GetTile (0,y)+ "][" + GetTile(1, y) + "][" + GetTile(2, y) + "]");
            Console.WriteLine("  A  B  C");
        }
        
        private char GetTile(int x, int y)
        {
            int type = grid[x][y];
            switch(type)
            {
                case 1: return 'X';
                case 2: return 'O';
            }

            return ' ';
        }
    }
}