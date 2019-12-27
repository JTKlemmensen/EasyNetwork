using EasyNetwork.Network.Abstract;
using EasyNetwork.Network.Attributes;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EasyNetwork.Network
{
    /// <summary>
    /// Constantly checks if the remote peer is connected. 
    /// If the remote peer is idle the connection is terminated.
    /// </summary>
    public class IdleChecker
    {
        /// <summary>
        /// Stores a bool for each subscribed connection, 
        /// </summary>
        private IDictionary<IObjectConnection, IdleInfo> Connections = new ConcurrentDictionary<IObjectConnection, IdleInfo>();
        private const long TicksPerMillisecond = 10000;
        private const long MillisecondPerSecond = 1000;
        private const long MaxConnectionIdle = 3 * TicksPerMillisecond * MillisecondPerSecond;
        private const long IdleCheckerCooldown = 1 * TicksPerMillisecond * MillisecondPerSecond;
        private const int RefreshRate = 50;
        private bool run;

        public IdleChecker()
        {
            run = true;
            new Task(Run, TaskCreationOptions.LongRunning).Start();
        }

        private void Run()
        {
            while(run)
            {
                var time = DateTime.Now.Ticks;
                foreach(var c in Connections)
                    if (c.Value.HasPonged)
                    {
                        if (time - c.Value.LastTicks > IdleCheckerCooldown)
                        {
                            c.Key.SendObject(new PingObject());
                            c.Value.LastTicks = time;
                            c.Value.HasPonged = false;
                        }
                    }
                    else if (time - c.Value.LastTicks > MaxConnectionIdle)
                        c.Key.Stop();

                Thread.Sleep(RefreshRate);
            }
        }

        public void Stop()
        {
            run = false;
        }

        [Connect]
        public void OnConnect(IObjectConnection connection)
        {
            Connections[connection] = new IdleInfo { LastTicks = DateTime.Now.Ticks, HasPonged = true };
        }

        [Disconnect]
        public void OnDisconnect(IObjectConnection connection)
        {
            Connections.Remove(connection);
        }

        [Command]
        public void OnPing(IObjectConnection connection, PingObject pingObject)
        {
            connection.SendObject(new PongObject());
        }

        [Command]
        public void OnPong(IObjectConnection connection, PongObject pongObject)
        {
            var info = Connections[connection];
            info.HasPonged = true;
            info.LastTicks = DateTime.Now.Ticks;
        }

        private class IdleInfo
        {
            public long LastTicks { get; set; }
            public bool HasPonged { get; set; }
        }
    }
}