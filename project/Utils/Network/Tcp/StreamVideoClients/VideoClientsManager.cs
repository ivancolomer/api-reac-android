using REAC_AndroidAPI.Utils.Network.Tcp.Common;
using REAC_AndroidAPI.Utils.Output;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace REAC_AndroidAPI.Utils.Network.Tcp.StreamVideoClients
{
    public class VideoClientsManager : ClientManager
    {
        private const int LOOP_MILLS = 1 * 60 * 1000; //1min
        private const int MAX_TIME_DISCONNECTED = 5 * 60 * 1000; //5min

        private HashSet<string> ValidIpAddress;

        public VideoClientsManager()
            : base(LOOP_MILLS)
        {
            ValidIpAddress = new HashSet<string>();

            Task.Run(async () =>
            {
                while(true)
                {
                    byte[] packet = Program.VideoStreamServer.TakePacket();
                    if (packet == null)
                    {
                        await Task.Delay(10);
                    }
                    else
                    {
                        foreach (var session in Clients)
                        {
                            session.Key.Send(packet);
                        }
                    }
                }
            });
        }

        public override void CheckSessions()
        {
            foreach (var session in Clients)
            {
                if (Time.GetTime() - ((VideoClient)session.Key).LastTimeWritten >= MAX_TIME_DISCONNECTED)
                    session.Key.Close();
            }
        }

        public override void StopClient(Client client)
        {
            base.StopClient(client);
            if (Clients.Count <= 0)
            {
                Task.Run(async() =>
                {
                    await Program.LockerDevicesManager.SendMessageToAllDevicesBlocking("stop_video_stream");
                });
            }
        }

        public override SocketListener CreateSocketListener()
        {
            return new SocketListener(new IPEndPoint(IPAddress.Any, DotNetEnv.Env.GetInt("TCP_VIDEO_LISTENER_PORT")), 100, new OnNewConnectionCallback(this.HandleIncomingConnection));
        }

        public bool AddIPAddress(string ipAddress)
        {
            return ValidIpAddress.Add(ipAddress);
        }

        public void RemoveIpAddress(string ipAddress)
        {
            ValidIpAddress.Remove(ipAddress);
        }

        public override void HandleIncomingConnection(Socket incomingSocket)
        {
            Logger.WriteLine("VideoClient connecting: " + ((IPEndPoint)incomingSocket.RemoteEndPoint).ToString().Split(':')[0], Logger.LOG_LEVEL.DEBUG);

            if (ValidIpAddress.Contains(((IPEndPoint)incomingSocket.RemoteEndPoint).ToString().Split(':')[0]))
            {
                Task.Run(async () =>
                {
                    List<string> responses = null;
                    try
                    {
                        responses = await Program.LockerDevicesManager.SendMessageToAllDevicesBlocking("start_video_stream");
                    }
                    catch (Exception)
                    {

                    }

                    if (responses != null && responses.Count > 0)
                    {
                        Clients.TryAdd(new VideoClient(this, incomingSocket), 0);
                        Logger.WriteLine("VideoClient connected: " + ((IPEndPoint)incomingSocket.RemoteEndPoint).ToString().Split(':')[0], Logger.LOG_LEVEL.DEBUG);
                    }
                    else
                    {
                        try
                        {
                            incomingSocket.Close();
                        }
                        catch (Exception)
                        {
                        }
                        try
                        {
                            incomingSocket.Dispose();
                        }
                        catch (Exception)
                        {

                        }
                    }

                });
            }
            else
            {
                try
                {
                    incomingSocket.Close();
                }
                catch (Exception)
                {
                }
                try
                {
                    incomingSocket.Dispose();
                }
                catch (Exception)
                {

                }
            }
        }
    }
}
