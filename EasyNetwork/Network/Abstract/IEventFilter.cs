﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace EasyNetwork.Network.Abstract
{
    /// <summary>
    /// Filters when an event should be run based on the generated Func
    /// </summary>
    public interface IEventFilter
    {
        /// <summary>
        /// Generates a function that defines whether or not a network event is executed.
        /// </summary>
        Func<IObjectConnection, bool> GenerateFunc(MethodInfo methodInfo);
    }
}