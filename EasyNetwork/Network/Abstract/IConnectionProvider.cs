using System;
using System.Collections.Generic;
using System.Text;

namespace EasyNetwork.Network.Abstract
{
    public interface IConnectionProvider
    {
        IObjectConnection Create(string ip, int port);
    }
}