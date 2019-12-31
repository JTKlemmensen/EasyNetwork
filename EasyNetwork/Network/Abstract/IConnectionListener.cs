using System;
using System.Collections.Generic;
using System.Text;

namespace EasyNetwork.Network.Abstract
{
    /// <summary>
    /// Listens for incoming connections.
    /// </summary>
    public interface IConnectionListener
    {
        /// <summary>
        /// Starts listening for incoming connections. When a remote peer connects <see cref="OnInboundConnection"/> is called.
        /// </summary>
        void Start();

        /// <summary>
        /// Stops listening for incoming connections. Calling <see cref="Stop"/> does not terminate existing connections.
        /// </summary>
        void Stop();

        /// <summary>
        /// Called before establishing a connection with the remote peer.
        /// </summary>
        event InboundConnection OnInboundConnection;
    }

    public delegate void InboundConnection(IObjectConnection con);
}