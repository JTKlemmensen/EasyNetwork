using EasyNetwork.Network.Abstract;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace EasyNetwork.Network
{
    /// <summary>
    /// Combines 2 EventFilters in to 1. Returns true if both return true or are null
    /// and returns false if either one returns false.
    /// </summary>
    public class AndEventFilter : IEventFilter
    {
        public IEventFilter Left { get; set; }
        public IEventFilter Right { get; set; }
        public Func<IObjectConnection,bool> GenerateFunc(MethodInfo methodInfo)
        {
            return (c) => (Left?.GenerateFunc(methodInfo).Invoke(c) ?? true) &&
                         (Right?.GenerateFunc(methodInfo).Invoke(c) ?? true);
        }
    }
}