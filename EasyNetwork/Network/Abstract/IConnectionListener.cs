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
        void Start();
        void Stop();
    }
}