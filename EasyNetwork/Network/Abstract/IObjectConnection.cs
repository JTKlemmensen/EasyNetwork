using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EasyNetwork.Network.Abstract
{
    /// <summary>
    /// Simplifies sending data to a remote peer
    /// </summary>
    public interface IObjectConnection
    {
        /// <summary>
        /// Returns the ip of the remote peer
        /// </summary>
        string Ip { get; }

        /// <summary>
        /// Sends an object to the remote peer
        /// </summary>
        /// <param name="o"></param>
        void SendObject(object o);

        /// <summary>
        /// Sends an object to the remote peer and returns the first received object of the given type.
        /// </summary>
        Task<ReturnType> SendObject<ReturnType>(object o);

        /// <summary>
        /// Establishes the connection to the remote peer.
        /// When the connection has been established, OnConnect events will be called and data can be received and sent.
        /// </summary>
        void Start();

        /// <summary>
        /// Stops the connection and will no longer listen for incoming data or be able to send data to the remote peer.
        /// </summary>
        void Stop();

        /// <summary>
        /// Add an event to be called when an object of type T is received.
        /// </summary>
        void OnCommand<T>(Action<IObjectConnection, T> command);

        /// <summary>
        /// Add an event to be called when the connection has succesfully been established to the remote peer.
        /// </summary>
        void OnConnect(Action<IObjectConnection> connect);

        /// <summary>
        /// Add an event to be called when <see cref="Stop"/> has been called and the connection no longer listens for incoming data.
        /// </summary>
        void OnDisconnect(Action<IObjectConnection> disconnect);
    }
}