﻿using ObjectNetwork.Network.Abstract;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace ObjectNetwork.Network
{
    /// <summary>
    /// Combines 2 EventFilters in to 1. Returns true if both return true or are null
    /// and returns false if either one returns false.
    /// </summary>
    public class AndEventFilter : IEventFilter
    {
        public IEventFilter Left { get; set; }
        public IEventFilter Right { get; set; }
        public Func<bool> GenerateFunc(MethodInfo methodInfo)
        {
            return () => (Left?.GenerateFunc(methodInfo).Invoke() ?? true) &&
                         (Right?.GenerateFunc(methodInfo).Invoke() ?? true);
        }
    }
}