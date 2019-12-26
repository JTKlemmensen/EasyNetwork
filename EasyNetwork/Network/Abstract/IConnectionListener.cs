using System;
using System.Collections.Generic;
using System.Text;

namespace EasyNetwork.Network.Abstract
{
    public interface IConnectionListener
    {
        void Start();
        void Stop();
    }
}