using ObjectNetwork.Network.Abstract;
using ObjectNetwork.Network.Formatters;
using System;
using System.Collections.Generic;
using System.Text;

namespace ObjectNetwork.Network
{
    public class ConnectionBuilder
    {
        private List<EventHandlerWrapper> EventHandlers { get; } = new List<EventHandlerWrapper>();
        public IDataFormatters DataFormatters { get; set; }

        public ConnectionBuilder()
        {
            DataFormatters = new DefaultDataFormatters();
        }

        public ConnectionBuilder AddEventHandler(object eventHandler, IEventFilter filter = null)
        {
            EventHandlers.Add(new EventHandlerWrapper {  EventHandler = eventHandler, Filter = filter });
            return this;
        }

        public ObjectConnection CreateClient(string ip, int port)
        {
            var connectionProvider = new SecureTcpConnectionProvider {Manager = GetManager(), DataFormatters= DataFormatters };
            return connectionProvider.Create(ip, port);
        }

        public IConnectionListener CreateServer(int port)
        {
            var connectionListener = new SecureTcpConnectionListener(port) { Manager = GetManager(), DataFormatters = DataFormatters };
            return connectionListener;
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