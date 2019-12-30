using EasyNetwork.Network.Abstract;
using EasyNetwork.Network.Formatters;
using System;
using System.Collections.Generic;
using System.Text;

namespace EasyNetwork.Network
{
    public class ConnectionBuilder
    {
        /// <summary>
        /// The DataFormatters Object used when creating a connection. <see cref="DefaultDataFormatters"/> is used by default.
        /// </summary>
        public IDataFormatters DataFormatters { get; set; }

        public ConnectionBuilder()
        {
            DataFormatters = new DefaultDataFormatters();
        }

        /// <summary>
        /// Creates a new client connection which connects to the given ip and port.
        /// </summary>
        public IObjectConnection CreateClient(string ip, int port)
        {
            return new SecureTcpConnectionProvider { DataFormatters= DataFormatters }.Create(ip, port);
        }

        /// <summary>
        /// Creates a new <see cref="IConnectionListener"/> which listens on the given port.
        /// </summary>
        public IConnectionListener CreateServer(int port)
        {
            return new SecureTcpConnectionListener(port) { DataFormatters = DataFormatters };
        }
    }
}