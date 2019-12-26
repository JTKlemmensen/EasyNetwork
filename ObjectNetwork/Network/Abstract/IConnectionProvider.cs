using System;
using System.Collections.Generic;
using System.Text;

namespace ObjectNetwork.Network.Abstract
{
    public interface IConnectionProvider
    {
        IObjectConnection Create(string ip, int port);
    }
}