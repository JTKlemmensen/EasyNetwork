using EasyNetwork.Network.Abstract;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace EasyNetwork.Network
{
    /// <summary>
    /// Combines 2 EventFilters in to 1. Returns true if either one return true or are null
    /// and returns false if both return false
    /// </summary>
    public class OrEventFilter : IEventFilter
    {
        public IEventFilter Left { get; set; }
        public IEventFilter Right { get; set; }

        Func<IObjectConnection, bool> IEventFilter.GenerateFunc(MethodInfo methodInfo)
        {
            return (c) => (Left?.GenerateFunc(methodInfo).Invoke(c) ?? true) ||
                         (Right?.GenerateFunc(methodInfo).Invoke(c) ?? true);
        }
    }
}