using EasyNetwork.Network.Abstract;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace EasyNetwork.Network
{
    /// <summary>
    /// Wraps a binary connection
    /// </summary>
    public class DefaultObjectConnection : IObjectConnection
    {
        private IDictionary<string, CommandLink> Commands = new Dictionary<string, CommandLink>();
        private ConnectLink Connect = null;
        private ConnectLink Disconnect = null;
        private IConnection connection;
        public IEventManager Manager { get; set; }
        public ISerializer Serializer { get; set; }
        private bool hasBeenStarted;
        private bool hasBeenStopped;

        public string Ip => connection.Ip;

        public DefaultObjectConnection(IConnection connection)
        {
            this.connection = connection;
            connection.OnConnected += RunConnects;
            connection.OnDisconnected += RunDisconnects;
            connection.OnDataReceived += Connection_OnMessageReceived;
        }

        private void Connection_OnMessageReceived(byte[] data)
        {
            var message = Serializer.Deserialize<NetworkMessage>(data);
            RunCommands(message.Name, message.Data);
            Manager.CallCommand(message.Name, message.Data, this);
        }

        private void RunConnects()
        {
            var link = Connect;
            while(link != null)
            {
                link.Action.Invoke(this);
                link = link.Next;
            }
        }

        private void RunDisconnects()
        {
            var link = Disconnect;
            while (link != null)
            {
                link.Action.Invoke(this);
                link = link.Next;
            }
        }

        private void RunCommands(string name, byte[] data)
        {
            if(Commands.TryGetValue(name, out CommandLink link))
                while(link != null)
                {
                    link.Action.Invoke(data);
                    link = link.Next;
                }
        }

        public void SendObject(object t)
        {
            var message = new NetworkMessage { Name= t.GetType().FullName, Data = Serializer.Serialize(t) };
            connection.SendData(Serializer.Serialize(message));
        }

        public void Stop()
        {
            if (hasBeenStopped)
                return;

            hasBeenStopped = true;
            hasBeenStarted = true;
            connection.OnDisconnected += () => Manager.CallDisconnect(this);
            connection.Stop();
        }

        public void Start()
        {
            if (hasBeenStarted)
                return;

            hasBeenStarted = true;
            connection.OnConnected += () => Manager.CallConnect(this);
            connection.Start();
        }

        public void OnCommand<T>(Action<IObjectConnection, T> command)
        {
            Action<byte[]> del = (data) => 
            {
                var serializedData = Serializer.Deserialize<T>(data);
                command.Invoke(this, serializedData);
            };

            var name = typeof(T).FullName;

            if (Commands.ContainsKey(name))
                Commands[name] = new CommandLink {Action=del, Next=Commands[name] };
            else
                Commands[name] = new CommandLink { Action = del, Next =null };
        }

        public void OnConnect(Action<IObjectConnection> connect)
        {
            Connect = new ConnectLink { Action = connect, Next = Connect };
        }

        public void OnDisconnect(Action<IObjectConnection> disconnect)
        {
            Disconnect = new ConnectLink { Action = disconnect, Next = Disconnect };
        }

        public async Task<E> SendObject<E>(object t)
        {
            E output = default;
            bool isDone = false;

            OnCommand<E>((o, e) => 
            {
                output = e;
                isDone = true;
            });

            SendObject(t);
            return await Task<E>.Run(async () =>
            {
                while (true)
                {
                    if (isDone)
                        break;
                    await Task.Delay(50);
                }

                return output;
            });
        }

        private class CommandLink
        {
            public Action<byte[]> Action { get; set; }
            public CommandLink Next { get; set; }
        }

        private class ConnectLink
        {
            public Action<IObjectConnection> Action { get; set; }
            public ConnectLink Next { get; set; }
        }
    }
}