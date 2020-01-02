using EasyNetwork.Network.Abstract;
using EasyNetwork.Network.Attributes;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace TicTacToe
{
    public class ServerManager
    {
        private object connectionLock = new object();
        private IObjectConnection connection = null;

        [Connect]
        public void OnClientConnected(IObjectConnection connection)
        {
            lock (connectionLock)
                if (this.connection != null)
                {
                    var manager = new TicTacToeManager(this.connection, connection);

                    this.connection.AddEventHandler(manager);
                    connection.AddEventHandler(manager);

                    // Between creating the TicTacToeManager and adding it as an eventhandler to each connection
                    // the other connection could disconnect and the other would be stuck in a ghost game
                    // so check if one the connections has disconnected and notify the manager.
                    if(this.connection.IsStopped)
                        manager.OnDisconnect(this.connection);

                    if (connection.IsStopped)
                        manager.OnDisconnect(connection);

                    this.connection.RemoveEventHandlers(this);
                    connection.RemoveEventHandlers(this);
                    this.connection = null;
                }
                else
                    this.connection = connection;
        }

        [Disconnect]
        public void OnClientDisconnected(IObjectConnection connection)
        {
            lock (connectionLock)
                if (this.connection == connection)
                    this.connection = null;
        }
    }
}