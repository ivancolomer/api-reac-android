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
        //private const int MAX_TIME_DISCONNECTED = 5 * 60 * 1000; //5min

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

        public override void StopClient(Client client)
        {
            ValidIpAddress.Remove(client.Address);
            base.StopClient(client);
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
            return SendMessage<byte[]>("get_live_image");
        }

        public Task<List<byte[]>> GetImageUserRecognition(uint userId, int photoId)
        {
            return SendMessage<byte[]>("get_photo_user|" + userId + "|" + photoId + "|");
        }

        public Task<List<string>> SendMessageToAllDevicesBlocking(string message)
        {
            return SendMessage<string>(message);
        }

        private async Task<List<T>> SendMessage<T>(string message)
        {
            List<T> responses = new List<T>(); // List of responses from locking devices to return

            /*
             * Dictionary of tasks that the server will be waiting for until locking devices 
             * send back an answer or until a specific timeout (2 seconds)
             */
            Dictionary<LockerDevice, Task<object>> tasks = new Dictionary<LockerDevice, Task<object>>();

            /* Send the message to all clients and add the task which we will be waiting for */
            foreach (var session in Clients)
            {
                TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
                ((LockerDevice)session.Key).BlockingSend(message, tcs);
                tasks.Add((LockerDevice)session.Key, tcs.Task);
            }

            /* Wait for all tasks to be completed or cancelled */
            await Task.WhenAll(tasks.Values);

            /* Get the results from tasks and add the completed ones to the list of responses */
            foreach (KeyValuePair<LockerDevice, Task<object>> keyValuePair in tasks)
            {
                Task<object> task = keyValuePair.Value;
                if (task.IsCompletedSuccessfully && !task.IsCanceled &&  task.Result != null && task.Result is T)
                    responses.Add((T)task.Result);
            }

            return responses;
        }
    }
}
