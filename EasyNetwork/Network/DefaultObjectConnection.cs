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
        private IDictionary<string, List<CommandEvent>> CommandEvents = new Dictionary<string, List<CommandEvent>>();
        private List<ConnectEvent> ConnectEvents = new List<ConnectEvent>();
        private List<ConnectEvent> DisconnectEvents = new List<ConnectEvent>();

        private readonly object connectLock = new object();
        private readonly object disconnectLock = new object();
        private readonly object commandsLock = new object();


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
            foreach (var e in ConnectEvents)
                e.Action.Invoke(this);
        }

        private void RunDisconnects()
        {
            foreach (var e in DisconnectEvents)
                e.Action.Invoke(this);
        }

        private void RunCommands(string name, byte[] data)
        {
            lock (commandsLock)
                if (CommandEvents.TryGetValue(name, out List<CommandEvent> events))
                    foreach(var e in events)
                        e.Action.Invoke(data);
        }

        public void SendObject(object obj)
        {
            var message = new NetworkMessage { Name= obj.GetType().FullName, Data = Serializer.Serialize(obj) };
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

        public void OnCommand<T>(Action<IObjectConnection, T> command, object creator = null)
        {
            lock (commandsLock)
            {
                Action<byte[]> del = (data) =>
                {
                    var serializedData = Serializer.Deserialize<T>(data);
                    command.Invoke(this, serializedData);
                };

                var name = typeof(T).FullName;

                if (!CommandEvents.ContainsKey(name))
                    CommandEvents[name] = new List<CommandEvent>();

                CommandEvents[name].Add(new CommandEvent { Creator = creator ?? command, Action = del });
            }
        }

        public void RemoveOnCommand<T>(object creator)
        {
            if (creator != null)
                lock (commandsLock)
                    if (CommandEvents.TryGetValue(typeof(T).FullName, out List<CommandEvent> es))
                        es.RemoveAll(p => p.Creator == creator);
        }
        public void RemoveOnCommand(object creator)
        {
            if(creator != null)
                lock (commandsLock)
                    foreach (var pair in CommandEvents)
                        pair.Value.RemoveAll(p => p.Creator == creator);
        }

        public void RemoveOnConnect(object creator)
        {
            if (creator != null)
                lock (connectLock)
                    ConnectEvents.RemoveAll(p => p.Creator == creator);
        }

        public void RemoveOnDisconnect(object creator)
        {
            if (creator != null)
                lock (disconnectLock)
                    DisconnectEvents.RemoveAll(p => p.Creator == creator);
        }

        public void OnConnect(Action<IObjectConnection> connect, object creator = null)
        {
            lock(connectLock)
                ConnectEvents.Add(new ConnectEvent { Action = connect, Creator=creator ?? connect});
        }

        public void OnDisconnect(Action<IObjectConnection> disconnect, object creator = null)
        {
            lock (disconnectLock)
                DisconnectEvents.Add(new ConnectEvent { Action = disconnect, Creator= creator ?? disconnect});
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

        private class CommandEvent
        {
            public Action<byte[]> Action { get; set; }
            public object Creator { get; set; }
        }

        private class ConnectEvent
        {
            public Action<IObjectConnection> Action { get; set; }
            public object Creator { get; set; }
        }
    }
}