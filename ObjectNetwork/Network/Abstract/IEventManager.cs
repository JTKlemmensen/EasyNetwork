using System;
using System.Collections.Generic;
using System.Text;

namespace ObjectNetwork.Network.Abstract
{
    public interface IEventManager
    {
        void CallConnect(ObjectConnection connection);
        void CallCommand(string protocol, object parameter, ObjectConnection connection);
        void CallDisconnect(ObjectConnection connection);
    }
}