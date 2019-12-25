using System;
using System.Collections.Generic;
using System.Text;

namespace ObjectNetwork.Network.Abstract
{
    public interface IConnectionListener
    {
        void Start();
        void Stop();
    }
}