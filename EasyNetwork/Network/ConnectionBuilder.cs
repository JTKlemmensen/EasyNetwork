using EasyNetwork.Network.Abstract;
using EasyNetwork.Network.Formatters;
using System;
using System.Collections.Generic;
using System.Text;

namespace EasyNetwork.Network
{
    public class ConnectionBuilder
    {
        private List<EventHandlerWrapper> EventHandlers { get; } = new List<EventHandlerWrapper>();
        /// <summary>
        /// The DataFormatters Object used when creating a connection. <see cref="DefaultDataFormatters"/> is used by default.
        /// </summary>
        public IDataFormatters DataFormatters { get; set; }

        public ConnectionBuilder()
        {
            DataFormatters = new DefaultDataFormatters();
        }

        /// <summary>
        /// Adds an eventhandler which will be used when creating a new connection. 
        /// If an <see cref="IEventFilter"/> has been specified, it will be used in conjunction with the eventhandler.
        /// </summary>
        public ConnectionBuilder AddEventHandler(object eventHandler, IEventFilter filter = null)
        {
            if (eventHandler != null)
                EventHandlers.Add(new EventHandlerWrapper {  EventHandler = eventHandler, Filter = filter });

            return this;
        }

        /// <summary>
        /// Creates a new client connection which connects to the given ip and port.
        /// </summary>
        public IObjectConnection CreateClient(string ip, int port)
        {
            return new SecureTcpConnectionProvider { Manager = GetManager(), DataFormatters= DataFormatters }.Create(ip, port);
        }

        /// <summary>
        /// Creates a new <see cref="IConnectionListener"/> which listens on the given port.
        /// </summary>
        public IConnectionListener CreateServer(int port)
        {
            return new SecureTcpConnectionListener(port) { Manager = GetManager(), DataFormatters = DataFormatters };
        }

        private IEventManager GetManager()
        {
            var manager = new ReflectionEventManager(DataFormatters.Serializer);
            foreach (var e in EventHandlers)
                manager.AddCommandHandler(e.EventHandler, e.Filter);

            return manager;
        }

        private class EventHandlerWrapper
        {
            public object EventHandler { get; set; }
            public IEventFilter Filter { get; set; }
        }
    }
}