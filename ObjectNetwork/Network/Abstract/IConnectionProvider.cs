using System;
using System.Collections.Generic;
using System.Text;

namespace ObjectNetwork.Network.Abstract
{
    public interface IConnectionProvider
    {
        ObjectConnection Create(string ip, int port);
    }
}