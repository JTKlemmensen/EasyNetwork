﻿using EasyNetwork.Network.Abstract;
using EasyNetwork.Network.Attributes;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace EasyNetwork.Network
{
    /// <summary>
    /// Using Reflection, objects are serialized and then all methods subscribed to the serialized object
    /// will be called
    /// </summary>
    [ObsoleteAttribute("Is no longer used by DefaultObjectConnection.", false)]
    public class ReflectionEventManager : IEventManager
    {
        private Dictionary<string, CommandEventObject> CommandObjects { get; } = new Dictionary<string, CommandEventObject>();
        private ConnectEventObject ConnectObject { get; set; }
        private ConnectEventObject DisconnectObject { get; set; }
        private ISerializer serializer;

        public ReflectionEventManager(ISerializer serializer)
        {
            this.serializer = serializer;
        }

        public void AddCommandHandler(object commandHandler, IEventFilter filter = null)
        {
            MethodInfo deserializerMethod = typeof(ISerializer).GetMethod(nameof(serializer.Deserialize));
            Type commandHandlerType = commandHandler.GetType();
            MethodInfo[] methods = commandHandlerType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var method in methods)
            {
                var connectAttr = method.GetCustomAttribute(typeof(Connect));
                var disconnectAttr = method.GetCustomAttribute(typeof(Disconnect));
                var commandAttr = method.GetCustomAttribute(typeof(Command));

                if (connectAttr != null)
                    AddConnectHandler(commandHandler,filter, method);
                if (disconnectAttr != null)
                    AddDisconnectHandler(commandHandler, filter, method);
                if (commandAttr != null)
                    AddCommandHandler(commandHandler, filter, method, deserializerMethod);
            }
        }

        private void AddConnectHandler(object commandHandler, IEventFilter filter, MethodInfo method)
        {
            var parameters = method.GetParameters();

            if (parameters.Length == 1 && parameters[0].ParameterType == typeof(IObjectConnection))
            {
                var connectObj = new ConnectEventObject()
                {
                    Action = method.CreateDelegate(typeof(Action<IObjectConnection>), commandHandler) as Action<IObjectConnection>,
                    CanExecute = filter?.GenerateFunc(method),
                };

                if (ConnectObject == null)
                    ConnectObject = connectObj;
                else
                {
                    connectObj.Next = ConnectObject;
                    ConnectObject = connectObj;
                }
            }
        }

        private void AddDisconnectHandler(object commandHandler, IEventFilter filter, MethodInfo method)
        {
            var parameters = method.GetParameters();

            if (parameters.Length == 1 && parameters[0].ParameterType == typeof(IObjectConnection))
            {
                var connectObj = new ConnectEventObject()
                {
                    Action = method.CreateDelegate(typeof(Action<IObjectConnection>), commandHandler) as Action<IObjectConnection>,
                    CanExecute = filter?.GenerateFunc(method),
                };

                if (ConnectObject == null)
                    DisconnectObject = connectObj;
                else
                {
                    connectObj.Next = DisconnectObject;
                    DisconnectObject = connectObj;
                }
            }
        }

        private void AddCommandHandler(object commandHandler, IEventFilter filter, MethodInfo method, MethodInfo deserializerMethod)
        {
            var methodParameters = method.GetParameters();
            if (methodParameters.Length != 2 || methodParameters[0].ParameterType != typeof(IObjectConnection))
                return;

            Type commandObjectType = methodParameters[1].ParameterType;

            var methodAction = Expression.GetActionType(typeof(IObjectConnection), commandObjectType);
            var deserializeMethInfo = deserializerMethod.MakeGenericMethod(commandObjectType);
            var deserializeFunc = Expression.GetFuncType(typeof(byte[]), commandObjectType);
            var commandObj = new CommandEventObject
            {
                CommandMethod = Delegate.CreateDelegate(methodAction, commandHandler, method),
                DeserializerMethod = Delegate.CreateDelegate(deserializeFunc, serializer, deserializeMethInfo),
                CanExecute = filter?.GenerateFunc(method),
            };

            CommandObjects[commandObjectType.FullName] = commandObj;
        }

        public void CallCommand(string protocol, object parameter, IObjectConnection connection)
        {
            if (CommandObjects.ContainsKey(protocol))
            {
                var commandObj = CommandObjects[protocol];
                if (commandObj.CanExecute?.Invoke(connection) ?? true)
                {
                    var deserializedObject = commandObj.DeserializerMethod((dynamic)parameter);
                    if (deserializedObject != null)
                        commandObj.CommandMethod(connection,deserializedObject);
                }
            }
        }

        public void CallConnect(IObjectConnection connection)
        {
            var previous = ConnectObject;
            while(previous != null)
            {
                if (previous.CanExecute?.Invoke(connection) ?? true)
                    previous.Action.Invoke(connection);

                previous = previous.Next;
            }
        }

        public void CallDisconnect(IObjectConnection connection)
        {
            var previous = DisconnectObject;
            while (previous != null)
            {
                if (previous.CanExecute?.Invoke(connection) ?? true)
                    previous.Action.Invoke(connection);

                previous = previous.Next;
            }
        }

        public class CommandEventObject
        {
            public dynamic CommandMethod { get; set; }
            public dynamic DeserializerMethod { get; set; }
            public Func<IObjectConnection, bool> CanExecute { get; set; }
            public CommandEventObject Next { get; set; }
        }

        private class ConnectEventObject
        {
            public Action<IObjectConnection> Action { get; set; }
            public Func<IObjectConnection, bool> CanExecute { get; set; }
            public ConnectEventObject Next { get; set; }
        }
    }
}