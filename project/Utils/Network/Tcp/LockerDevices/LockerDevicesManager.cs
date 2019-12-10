using REAC_AndroidAPI.Utils.Network.Tcp.Common;
using REAC_AndroidAPI.Utils.Output;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

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

        public async Task<List<byte[]>> GetLiveImageFromLockingDevices()
        {
            List<byte[]> responses = new List<byte[]>();
            Dictionary<LockerDevice, Task<string>> tasks = new Dictionary<LockerDevice, Task<string>>();

            foreach (var session in Clients)
            {
                TaskCompletionSource<string> tcs = new TaskCompletionSource<string>();
                ((LockerDevice)session.Key).BlockingSend("get_live_image", tcs);
                tasks.Add((LockerDevice)session.Key, tcs.Task);
            }

            await Task.WhenAll(tasks.Values);

            foreach (var task in tasks)
            {
                try
                {
                    if (task.Value.IsCompletedSuccessfully && task.Value.Result != null && task.Value.Result == "image_sent")
                        responses.Add(task.Key.LastImageSent);
                }
                catch (Exception)
                {

                }
            }

            return responses;
        }

        public async Task<List<string>> SendMessageToAllDevicesBlocking(string message)
        {
            List<string> responses = new List<string>();
            List<Task<string>> tasks = new List<Task<string>>();

            foreach (var session in Clients)
            {
                //Logger.WriteLineWithHeader("Message sent: " + message, session.Key.Address, Logger.LOG_LEVEL.DEBUG);
                TaskCompletionSource<string> tcs = new TaskCompletionSource<string>();
                ((LockerDevice)session.Key).BlockingSend(message, tcs);
                tasks.Add(tcs.Task);
            }

            await Task.WhenAll(tasks);

            foreach(var task in tasks)
            {
                try
                {
                    if (task.IsCompletedSuccessfully)
                        responses.Add(task.Result);
                }
                catch(Exception)
                {

                }
            }

            return responses;
        }
    }
}
