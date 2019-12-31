using EasyNetwork.Network.Abstract;
using EasyNetwork.Network.Attributes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EasyNetwork.Network
{
    /// <summary>
    /// Wraps a binary connection
    /// </summary>
    public class DefaultObjectConnection : IObjectConnection
    {
        private IDictionary<string, IEventList> CommandEvents = new Dictionary<string, IEventList>();
        private List<ConnectEvent> ConnectEvents = new List<ConnectEvent>();
        private List<ConnectEvent> DisconnectEvents = new List<ConnectEvent>();

        private readonly object connectLock = new object();
        private readonly object disconnectLock = new object();
        private readonly object commandsLock = new object();

        private IConnection connection;
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
        }

        private void RunConnects()
        {
            ConnectEvent[] events = null;
            lock (connectLock)
                events = ConnectEvents.ToArray();

            foreach (var e in events)
                e.Action.Invoke(this);
        }

        private void RunDisconnects()
        {
            ConnectEvent[] events = null;
            lock (disconnectLock)
                events = DisconnectEvents.ToArray();

            foreach (var e in events)
                e.Action.Invoke(this);
        }

        private void RunCommands(string name, byte[] data)
        {
            IEventList eventList = null;
            lock (commandsLock)
                if (CommandEvents.ContainsKey(name))
                {
                    eventList = CommandEvents[name];
                    eventList.Prepare();
                }

            eventList?.Invoke(data);
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
            connection.Stop();
        }

        public void Start()
        {
            if (hasBeenStarted)
                return;

            hasBeenStarted = true;
            connection.Start();
        }

        public void OnCommand<T>(Action<IObjectConnection, T> command, object creator = null)
        {
            lock (commandsLock)
            {
                Action<T> del = (data) => command.Invoke(this, data);;

                Func<byte[], T> aa = (data) => Serializer.Deserialize<T>(data);

                var name = typeof(T).FullName;
                EventList<T> eventList = null;

                if (CommandEvents.ContainsKey(name))
                    eventList = CommandEvents[name] as EventList<T>;
                else
                {
                    eventList = new EventList<T>();
                    CommandEvents[name] = eventList;
                    eventList.Deserialize = (data) => Serializer.Deserialize<T>(data);
                }

                eventList.Add(new CommandEvent<T> { Creator = creator ?? command, Action = del });
            }
        }

        public void RemoveOnCommand<T>(object creator)
        {
            if (creator != null)
                lock (commandsLock)
                {
                    if (CommandEvents.TryGetValue(typeof(T).FullName, out IEventList es))
                        es.RemoveAll(creator);
                }
        }

        public void RemoveOnCommand(object creator)
        {
            if (creator != null)
                lock (commandsLock)
                {
                    foreach (var pair in CommandEvents)
                        pair.Value.RemoveAll(creator);
                }
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

            Action<IObjectConnection,E> del = (o, e) =>
            {
                output = e;
                isDone = true;
            };

            OnCommand<E>(del);

            SendObject(t);
            return await Task<E>.Run(async () =>
            {
                while (true)
                {
                    if (isDone)
                        break;
                    await Task.Delay(5);
                }
                RemoveOnCommand(del);
                return output;
            });
        }

        public void AddEventHandler(object handler, IEventFilter filter = null)
        {
            Type commandHandlerType = handler.GetType();
            MethodInfo[] methods = commandHandlerType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var method in methods)
            {
                var connectAttr = method.GetCustomAttribute(typeof(Connect));
                var disconnectAttr = method.GetCustomAttribute(typeof(Disconnect));
                var commandAttr = method.GetCustomAttribute(typeof(Command));

                if (connectAttr != null)
                    AddConnectHandlerWithAttribute(handler, filter, method);
                if (disconnectAttr != null)
                    AddDisconnectHandlerWithAttribute(handler, filter, method);
                if (commandAttr != null)
                    AddCommandHandlerWithAttribute(handler, filter, method);
            }
        }

        /// <summary>
        /// Service method for AddEventHandler. Adds a method with a connect attribute as connect eventhandler.
        /// </summary>
        private void AddDisconnectHandlerWithAttribute(object handler, IEventFilter filter, MethodInfo method)
        {
            var methodParameters = method.GetParameters();
            if (methodParameters.Length != 1 || methodParameters[0].ParameterType != typeof(IObjectConnection))
                return;

            var exp = Expression.GetActionType(typeof(IObjectConnection));

            var delegat = method.CreateDelegate(exp, handler);

            typeof(IObjectConnection)
            .GetMethod("OnDisconnect")
            .Invoke(this, new object[] { delegat, handler });
        }

        /// <summary>
        /// Service method for AddEventHandler. Adds a method with a connect attribute as connect eventhandler.
        /// </summary>
        private void AddConnectHandlerWithAttribute(object handler, IEventFilter filter, MethodInfo method)
        {
            var methodParameters = method.GetParameters();
            if (methodParameters.Length != 1 || methodParameters[0].ParameterType != typeof(IObjectConnection))
                return;

            var exp = Expression.GetActionType(typeof(IObjectConnection));

            var delegat = method.CreateDelegate(exp, handler);

            typeof(IObjectConnection)
            .GetMethod("OnConnect")
            .Invoke(this, new object[] { delegat, handler });
        }

        /// <summary>
        /// Service method for AddEventHandler. Adds a method with a command attribute as command eventhandler.
        /// </summary>
        private void AddCommandHandlerWithAttribute(object handler, IEventFilter filter, MethodInfo method)
        {
            var methodParameters = method.GetParameters();
            if (methodParameters.Length != 2 || methodParameters[0].ParameterType != typeof(IObjectConnection))
                return;

            var parameter1 = methodParameters[1].ParameterType;
            var exp = Expression.GetActionType(typeof(IObjectConnection), parameter1);
            var delegat = method.CreateDelegate(exp, handler);

            typeof(IObjectConnection)
            .GetMethod("OnCommand")
            .MakeGenericMethod(parameter1)
            .Invoke(this, new object[] { delegat, handler });
        }

        public void RemoveEventHandlers(object creator)
        {
            RemoveOnCommand(creator);
            RemoveOnConnect(creator);
            RemoveOnDisconnect(creator);
        }

        private class CommandEvent<T> : BaseEvent
        {
            public Action<T> Action { get; set; }
        }

        private class ConnectEvent : BaseEvent
        {
            public Action<IObjectConnection> Action { get; set; }
        }

        private abstract class BaseEvent
        {
            public object Creator { get; set; }
        }

        private class EventList<E> : IEventList
        {
            private List<CommandEvent<E>> Events { get; set; } = new List<CommandEvent<E>>();
            private CommandEvent<E>[] threadSafeArray;
            public Func<byte[],E> Deserialize { get; set; }

            public void Invoke(byte[] data)
            {
                E deserializedObj = Deserialize.Invoke(data);

                if (threadSafeArray != null)
                    for (int i = 0; i < threadSafeArray.Length; i++)
                        threadSafeArray[i].Action.Invoke(deserializedObj);
            }

            public void Prepare()
            {
                threadSafeArray = Events.ToArray();
            }

            public void RemoveAll(object creator)
            {
                Events.RemoveAll((e) => e.Creator == creator);
            }

            public void Add(CommandEvent<E> commandEvent)
            {
                Events.Add(commandEvent);
            }
        }

        private interface IEventList
        {
            void Prepare();
            void Invoke(byte[] data);
            void RemoveAll(object creator);
        }
    }
}