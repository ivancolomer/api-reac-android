using REAC_AndroidAPI.Utils.Network.Tcp.Common;
using REAC_AndroidAPI.Utils.Output;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace REAC_AndroidAPI.Utils.Network.Tcp.LockerDevices
{
    public class LockerDevicesManager : ClientManager
    {
        private const int LOOP_MILLS = 1 * 60 * 1000; //1min
        private const int MAX_TIME_DISCONNECTED = 5 * 60 * 1000; //5min

        private HashSet<string> ValidIpAddress;

        public LockerDevicesManager()
            : base(LOOP_MILLS)
        {
            ValidIpAddress = new HashSet<string>();
        }

        public override void CheckSessions()
        {
            /*foreach (var session in Clients)
            {
                if (Time.GetTime() - ((LockerDevice)session.Key).LastTimeReaden >= MAX_TIME_DISCONNECTED) 
                {
                    ValidIpAddress.Remove(session.Key.Address);
                    session.Key.Close();
                }
            }*/
        }

        public override SocketListener CreateSocketListener()
        {
            return new SocketListener(new IPEndPoint(IPAddress.Any, DotNetEnv.Env.GetInt("TCP_LOCKER_LISTENER_PORT")), 100, new OnNewConnectionCallback(this.HandleIncomingConnection));
        }

        public override void HandleIncomingConnection(Socket incomingSocket)
        {
            Logger.WriteLine("Locker device connected: " + ((IPEndPoint)incomingSocket.RemoteEndPoint).ToString().Split(':')[0], Logger.LOG_LEVEL.DEBUG);
            LockerDevice d = new LockerDevice(this, incomingSocket);
            Clients.TryAdd(d, 0);
            ValidIpAddress.Add(d.Address);
        }

        public bool IsIPAddressValid(string ip)
        {
            return ValidIpAddress.Contains(ip);
        }

        public bool SendMessageToAllDevices(string message)
        {
            bool sentAtleastOnce = false;
            foreach (var session in Clients)
            {
                Logger.WriteLine("Message sent: " + message, Logger.LOG_LEVEL.DEBUG);
                session.Key.Send(message);
                sentAtleastOnce = true;
            }
            return sentAtleastOnce;
        }
    }
}
