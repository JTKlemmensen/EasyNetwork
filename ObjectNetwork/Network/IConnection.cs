using System;
using System.Collections.Generic;
using System.Text;

namespace ObjectNetwork.Network
{
    /// <summary>
    /// Represents a very low level connection to a remote peer
    /// </summary>
    public interface IConnection
    {
        /// <summary>
        /// Setup initialization code and start listening for incomming data
        /// </summary>
        /// 
        void Start();
        /// <summary>
        /// Stops listenings to incomming data
        /// </summary>
        /// 
        void Stop();
        /// <summary>
        /// Sends a byte array to the remote peer
        /// </summary>
        void SendData(byte[] data);

        /// <summary>
        /// Called when data has been received from the remote peer
        /// </summary>
        event DataReceived OnDataReceived;

        /// <summary>
        /// Called when no longer listens for incoming data
        /// </summary>
        event Disconnected OnDisconnected;

        /// <summary>
        /// Called when succesfully connected to a remote peer and is ready to receive and send data
        /// </summary>
        event Connected OnConnected;
    }

    public delegate void DataReceived(byte[] data);
    public delegate void Connected();
    public delegate void Disconnected();
}