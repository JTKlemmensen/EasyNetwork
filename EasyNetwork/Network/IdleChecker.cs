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
        /// <summary>
        /// Allowed delay in Ticks between sending out a ping and receing a pong from the remote peer
        /// </summary>
        private long MaxIdleDelay { get; set; }
        /// <summary>
        /// Delay in Ticks between receiving a pong and sending out a new ping
        /// </summary>
        private long IdleCheckerCooldown { get; set; }
        private const int RefreshRate = 50;
        private bool run;

        public IdleChecker(int maxConnectionIdle = 3, int idleCooldown = 1)
        {
            MaxIdleDelay = maxConnectionIdle * TicksPerMillisecond * MillisecondPerSecond;
            IdleCheckerCooldown = idleCooldown * TicksPerMillisecond * MillisecondPerSecond;

            run = true;
            new Task(Run, TaskCreationOptions.LongRunning).Start();
        }

        private async void Run()
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
                    else if (time - c.Value.LastTicks > MaxIdleDelay)
                        c.Key.Stop();

                await Task.Delay(RefreshRate);
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