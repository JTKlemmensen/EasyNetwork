using System;
using System.Collections.Generic;
using System.Text;

namespace EasyNetwork.Network.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class Command : System.Attribute
    {
        public Command()
        {

        }
    }
}