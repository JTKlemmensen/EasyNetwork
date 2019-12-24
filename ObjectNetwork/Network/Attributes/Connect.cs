using System;
using System.Collections.Generic;
using System.Text;

namespace ObjectNetwork.Network.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class Connect : System.Attribute
    {
        public Connect()
        {

        }
    }
}