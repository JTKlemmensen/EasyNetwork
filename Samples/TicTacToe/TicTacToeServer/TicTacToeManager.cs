using EasyNetwork.Network.Abstract;
using EasyNetwork.Network.Attributes;
using System;
using System.Collections.Generic;
using System.Text;
using TicTacToeEntities;

namespace TicTacToe
{
    public class TicTacToeManager
    {
        private IObjectConnection player1;
        private IObjectConnection player2;
        private IObjectConnection playerTurn;
        private object[][] grid = new object[3][];

        public TicTacToeManager(IObjectConnection player1, IObjectConnection player2)
        {
            for (int x = 0; x < grid.Length; x++)
                grid[x] = new object[3];
            this.player1 = player1;
            this.player2 = player2;
            StartGame();
        }

        private void StartGame()
        {
            RandomTurn();
            player1.SendObject(new GameStarted { YourTurn = player1 == playerTurn });
            player2.SendObject(new GameStarted { YourTurn = player2 == playerTurn });
        }

        static Random r = new Random();
        private void RandomTurn()
        {
            if (r.Next(2) == 1)
                playerTurn = player1;
            else
                playerTurn = player2;
        }

        [Command]
        public void OnMove(IObjectConnection connection, Move move)
        {
            if (connection != playerTurn)
                return;

            if (CanPlace(move.X, move.Y))
            {
                Place(move.X, move.Y);
                if (HasPlayerWon())
                    EndGame();
                else
                    NextPlayerTurn(move.X, move.Y);
            }
        }

        [Disconnect]
        public void OnDisconnect(IObjectConnection connection)
        {
            if (connection == player1)
                player2.SendObject(new YouWon());
            else
                player1.SendObject(new YouWon());

            EndGame();
        }

        private bool CanPlace(int x, int y)
        {
            if(x >=0 && x <3 &&
                y >= 0 && y < 3)
                return grid[x][y] == null;
            return false;
        }

        private void Place(int x, int y)
        {
            grid[x][y] = playerTurn;
        }

        private bool HasPlayerWon()
        {
            return WonDiagonally() || WonVertically() || WonHorizontally();
        }

        private bool WonDiagonally()
        {
            return (grid[0][2] == playerTurn && grid[1][1] == playerTurn && grid[2][0] == playerTurn) ||
                   (grid[0][0] == playerTurn && grid[1][1] == playerTurn && grid[2][2] == playerTurn);
        }

        private bool WonVertically()
        {
            for (int y = 0; y < 3; y++)
                if (grid[0][y] == playerTurn &&
                    grid[1][y] == playerTurn && 
                    grid[2][y] == playerTurn)
                    return true;

            return false;
        }

        private bool WonHorizontally()
        {
            for (int x = 0; x < 3; x++)
                if (grid[x][0] == playerTurn &&
                   grid[x][1] == playerTurn &&
                   grid[x][2] == playerTurn)
                    return true;

            return false;
        }

        private void NextPlayerTurn(int x, int y)
        {
            if (playerTurn == player1)
                playerTurn = player2;
            else
                playerTurn = player1;

            playerTurn.SendObject(new YourTurn { X = x, Y = y });
        }

        private void EndGame()
        {
            InformPlayers();
            player1.RemoveEventHandlers(this);
            player2.RemoveEventHandlers(this);
            player1.Stop();
            player2.Stop();
        }

        private void InformPlayers()
        {
            if (playerTurn != player1)
                player1.SendObject(new YouLost());
            else
                player2.SendObject(new YouLost());

            playerTurn.SendObject(new YouWon());
        }
    }
}