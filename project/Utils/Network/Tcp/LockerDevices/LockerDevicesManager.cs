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

        public Task<List<byte[]>> GetLiveImageFromLockingDevices()
        {
            return GetImageFromLockingDevices("get_live_image");
        }

        public Task<List<byte[]>> GetImageUserRecognition(uint userId, int photoId)
        {
            return GetImageFromLockingDevices("get_photo_user|" + userId + "|" + photoId + "|");
        }

        private async Task<List<byte[]>> GetImageFromLockingDevices(string message)
        {
            List<byte[]> responses = new List<byte[]>();
            Dictionary<LockerDevice, Task<object>> tasks = new Dictionary<LockerDevice, Task<object>>();

            foreach (var session in Clients)
            {
                TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
                ((LockerDevice)session.Key).BlockingSend(message, tcs);
                tasks.Add((LockerDevice)session.Key, tcs.Task);
            }

            await Task.WhenAll(tasks.Values);

            foreach (var task in tasks)
            {
                try
                {
                    if (task.Value.IsCompletedSuccessfully && task.Value.Result != null && task.Value.Result is byte[])
                        responses.Add((byte[])task.Value.Result);
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
            List<Task<object>> tasks = new List<Task<object>>();

            foreach (var session in Clients)
            {
                //Logger.WriteLineWithHeader("Message sent: " + message, session.Key.Address, Logger.LOG_LEVEL.DEBUG);
                TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
                ((LockerDevice)session.Key).BlockingSend(message, tcs);
                tasks.Add(tcs.Task);
            }

            await Task.WhenAll(tasks);

            foreach(var task in tasks)
            {
                try
                {
                    if (task.IsCompletedSuccessfully && task.Result != null && task.Result is string)
                        responses.Add((string)task.Result);
                }
                catch(Exception)
                {

                }
            }

            return responses;
        }
    }
}
