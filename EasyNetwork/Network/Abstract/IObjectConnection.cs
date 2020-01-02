using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using EasyNetwork.Network.Attributes;
    
namespace EasyNetwork.Network.Abstract
{
    /// <summary>
    /// Simplifies sending data to a remote peer
    /// </summary>
    public interface IObjectConnection
    {
        bool IsStopped { get; }

        /// <summary>
        /// Returns the ip of the remote peer
        /// </summary>
        string Ip { get; }

        /// <summary>
        /// Sends an object to the remote peer
        /// </summary>
        /// <param name="o"></param>
        void SendObject(object o);

        /// <summary>
        /// Sends an object to the remote peer and returns the first received object of the given type.
        /// </summary>
        Task<ReturnType> SendObject<ReturnType>(object o);

        /// <summary>
        /// Establishes the connection to the remote peer.
        /// When the connection has been established, OnConnect events will be called and data can be received and sent.
        /// </summary>
        Task Start();

        /// <summary>
        /// Stops the connection and will no longer listen for incoming data or be able to send data to the remote peer.
        /// </summary>
        void Stop();

        /// <summary>
        /// Adds an eventhandler for each method in the eventhandler that uses an event attribute such as <see cref="Command"/>, <see cref="Connect"/> and <see cref="Disconnect"/>. 
        /// The method structure must comply with the expected structure or it will be ignored.
        /// If an <see cref="IEventFilter"/> has been specified, it will be used in conjunction with the eventhandler.
        /// </summary>
        void AddEventHandler(object handler, IEventFilter filter = null);

        /// <summary>
        /// Removes all eventhandlers that has the given creator object. 
        /// </summary>
        void RemoveEventHandlers(object creator);

        /// <summary>
        /// Add an eventhandler which will be called when an object of type T is received.
        /// Creator is used when removing the event. If the creator is null, the command is used as creator.
        /// </summary>
        void OnCommand<T>(Action<IObjectConnection, T> command, Func<IObjectConnection, bool> canRun=null, object creator = null);

        /// <summary>
        /// Add an eventhandler which will be called when the connection has succesfully been established to the remote peer.
        /// Creator is used when removing the event. If the creator is null, the command is used as creator.
        /// </summary>
        void OnConnect(Action<IObjectConnection> connect, Func<IObjectConnection, bool> canRun = null, object creator = null);

        /// <summary>
        /// Add an eventhandler which will be called when <see cref="Stop"/> has been called and the connection no longer listens for incoming data.
        /// Creator is used when removing the event. If the creator is null, the command is used as creator.
        /// </summary>
        void OnDisconnect(Action<IObjectConnection> disconnect, Func<IObjectConnection, bool> canRun = null, object creator = null);

        /// <summary>
        /// Removes all command eventhandlers that has the given creator object.
        /// </summary>
        void RemoveOnCommand(object creator);

        /// <summary>
        /// Removes all command eventhandlers that has the given creator object and is subscribed to the type T.
        /// </summary>
        void RemoveOnCommand<T>(object creator);

        /// <summary>
        /// Removes all connect eventhandlers that has the given creator object.
        /// </summary>
        void RemoveOnConnect(object creator);

        /// <summary>
        /// Removes all disconnect eventhandlers that has the given creator object.
        /// </summary>
        void RemoveOnDisconnect(object creator);
    }
}