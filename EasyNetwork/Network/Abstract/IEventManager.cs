using System;
using System.Collections.Generic;
using System.Text;

namespace EasyNetwork.Network.Abstract
{
    public interface IEventManager
    {
        void AddCommandHandler(object commandHandler, IEventFilter filter = null);
        void CallConnect(IObjectConnection connection);
        void CallCommand(string protocol, object parameter, IObjectConnection connection);
        void CallDisconnect(IObjectConnection connection);
    }
}