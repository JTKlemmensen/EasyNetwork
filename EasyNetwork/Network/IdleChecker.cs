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
        private ConcurrentDictionary<DefaultObjectConnection, IdleInfo> Connections = new ConcurrentDictionary<DefaultObjectConnection, IdleInfo>();
        private const int MaxConnectionIdle = 5000;
        private const int IdleCheckerCooldown = 1000;
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

                Thread.Sleep(IdleCheckerCooldown);
            }
        }

        public void Stop()
        {
            run = false;
        }

        [Connect]
        public void OnConnect(DefaultObjectConnection connection)
        {
            Task.Run(() =>
            {
                while (!Connections.TryAdd(connection, new IdleInfo { LastTicks = DateTime.Now.Ticks, HasPonged = true }))
                    Task.Delay(RefreshRate);
            });
        }

        [Disconnect]
        public void OnDisconnect(DefaultObjectConnection connection)
        {
            Task.Run(() =>
            {
                while (!Connections.TryRemove(connection, out IdleInfo info))
                    Task.Delay(RefreshRate);
            });
        }

        [Command]
        public void OnPing(DefaultObjectConnection connection, PingObject pingObject)
        {
            connection.SendObject(new PongObject());
        }

        [Command]
        public void OnPong(DefaultObjectConnection connection, PongObject pongObject)
        {
            Task.Run(() =>
            {
                while (true)
                {
                    if(Connections.TryGetValue(connection, out IdleInfo info))
                    {
                        info.HasPonged = true;
                        info.LastTicks = DateTime.Now.Ticks;
                        break;
                    }
                    Task.Delay(RefreshRate);
                }     
            });
        }

        private class IdleInfo
        {
            public long LastTicks { get; set; }
            public bool HasPonged { get; set; }
        }
    }
}