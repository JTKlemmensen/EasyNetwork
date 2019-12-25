using ObjectNetwork.Network.Attributes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ObjectNetwork.Network
{
    public class IdleChecker
    {
        private const double MaxConnectionIdle = 3;
        private const double IdleCheckerCooldown = 3;
        private const int UpdateRate = 100;
        private ObjectConnection connection;
        private Stopwatch stopWatch;
        private bool HasPonged;
        private bool run;

        public IdleChecker(ObjectConnection connection)
        {
            this.connection = connection;
            stopWatch = new Stopwatch();
        }

        private void Run()
        {
            run = true;
            OnPong(new PongObject());
            while (run)
                if (HasPonged)
                {
                    if (!IsOnCooldown())
                        Ping();
                    else
                        Thread.Sleep(UpdateRate);
                }
                else
                {
                    if (IsConnectionIdle())
                    {
                        connection.Stop();
                        run = false;
                    }
                    else
                        Thread.Sleep(UpdateRate);
                }
        }

        private bool IsOnCooldown()
        {
            return stopWatch.Elapsed.TotalSeconds <= IdleCheckerCooldown;
        }

        private bool IsConnectionIdle()
        {
            return stopWatch.Elapsed.TotalSeconds > MaxConnectionIdle;
        }

        private void Ping()
        {
            HasPonged = false;
            stopWatch.Reset();
            stopWatch.Start();
            connection.SendObject(new PingObject());
        }

        [Connect]
        public void OnConnect(ObjectConnection connection)
        {
            Task.Run(Run);
        }

        [Disconnect]
        public void OnDisconnect(ObjectConnection connection)
        {
            run = false;
        }

        [Command]
        public void OnPing(PingObject pingObject)
        {
            connection.SendObject(new PongObject());
        }

        [Command]
        public void OnPong(PongObject pongObject)
        {
            HasPonged = true;
            stopWatch.Reset();
            stopWatch.Start();
        }
    }
}