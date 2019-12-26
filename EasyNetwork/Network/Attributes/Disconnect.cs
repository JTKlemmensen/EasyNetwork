using System;
using System.Collections.Generic;
using System.Text;

namespace EasyNetwork.Network.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class Disconnect : System.Attribute
    {
        public Disconnect()
        {

        }
    }
}