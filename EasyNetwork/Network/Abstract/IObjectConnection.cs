using System;
using System.Collections.Generic;
using System.Text;

namespace EasyNetwork.Network.Abstract
{
    /// <summary>
    /// Simplifies sending data to a remote peer
    /// </summary>
    public interface IObjectConnection
    {
        string Ip { get; }
        void SendObject<T>(T t);
        void Start();
        void Stop();
        void OnCommand<T>(Action<IObjectConnection, T> command);
        void OnConnect(Action<IObjectConnection> connect);
        void OnDisconnect(Action<IObjectConnection> disconnect);
    }
}