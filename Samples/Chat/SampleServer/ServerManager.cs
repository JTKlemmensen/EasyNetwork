using EasyNetwork.Network.Abstract;
using EasyNetwork.Network.Attributes;
using NetworkEntities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace SampleServer
{
    public class ServerManager
    {
        // Connection : Name
        IDictionary<IObjectConnection, string> clients = new ConcurrentDictionary<IObjectConnection, string>();

        private void BroadCast<T>(T obj)
        {
            foreach (var client in clients)
                client.Key.SendObject(obj);
        }

        [Connect]
        public void OnClientConnected(IObjectConnection connection)
        {
            var name = GenerateUniqueName();
            clients[connection] = name;
            BroadCast(new ClientConnected { Name = name });
        }
         
        [Command]
        public void OnChatMessageReceived(IObjectConnection connection, string message)
        {
            var name = clients[connection];
            BroadCast(new Message
                {
                    SenderName = name,
                    Content = message
                });
        }

        [Disconnect]
        public void OnClientDisconnected(IObjectConnection connection)
        {
            var name = clients[connection];
            clients.Remove(connection);
            BroadCast(new ClientDisconnected
            {
                Name = name
            });
        }

        static int id = 0;
        static string GenerateUniqueName()
        {
            return "Client" + id++;
        }
    }
}