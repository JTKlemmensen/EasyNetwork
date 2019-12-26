using System;
using System.Collections.Generic;
using System.Text;

namespace ObjectNetwork.Network.Abstract
{
    public interface IEventManager
    {
        void AddCommandHandler(object commandHandler, IEventFilter filter = null);
        void CallConnect(DefaultObjectConnection connection);
        void CallCommand(string protocol, object parameter, DefaultObjectConnection connection);
        void CallDisconnect(DefaultObjectConnection connection);
    }
}