using REAC_AndroidAPI;
using REAC_AndroidAPI.Utils;
using REAC_AndroidAPI.Utils.Network;
using REAC_AndroidAPI.Utils.Network.Tcp.StreamVideoClients;
using REAC_AndroidAPI.Utils.Output;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;

namespace REAC2_AndroidAPI.Utils.Network.Udp
{
    public class VideoStreamServer
    {
        public bool stopListeningTo;
        private UdpClient udpClient;

        private long lastPacketDate = 0;

        private BlockingCollection<byte[]> packetCollection = new BlockingCollection<byte[]>(2048);

        public VideoStreamServer()
        {
            stopListeningTo = false;

            udpClient = new UdpClient(DotNetEnv.Env.GetInt("UDP_VIDEO_STREAM_PORT"));
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, 1);

            udpClient.BeginReceive(new AsyncCallback(ReceiveCallback), null);
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                IPEndPoint from = new IPEndPoint(0, 0);
                byte[] receiveBytes = udpClient.EndReceive(ar, ref from);

                if(Program.LockerDevicesManager != null && Program.LockerDevicesManager.IsIPAddressValid(from.ToString().Split(':')[0]))
                {
                    try
                    {
                        long dateNow = Time.GetTime();
                        if(lastPacketDate != 0)
                        {
                            long difference = dateNow - lastPacketDate;
                            //Logger.WriteLineWithHeader(difference.ToString(), "video_bytes", Logger.LOG_LEVEL.DEBUG);
                        }
                        lastPacketDate = dateNow;

                        if(Program.VideoClientsManager != null && Program.VideoClientsManager.Clients.Count > 0)
                            packetCollection.TryAdd(receiveBytes);
                    }
                    catch (Exception)
                    {

                    }
                }

                //Logger.WriteLineWithHeader($"Received from {ipReceiver}: {receiveString}", "STREAM", Logger.LOG_LEVEL.DEBUG);

                if (!stopListeningTo)
                {
                    udpClient.BeginReceive(new AsyncCallback(ReceiveCallback), null);
                }
                else
                {
                    try
                    {
                        udpClient.Close();
                        udpClient.Dispose();
                    }
                    catch(Exception)
                    {

                    }
                }
            }
            catch (SocketException)
            {
            }
            catch (ObjectDisposedException)
            {
            }
            catch (Exception e)
            {
                Logger.WriteLine("Exception while ReceivedCallback from VideoStreamServer: " + e.ToString(), Logger.LOG_LEVEL.ERROR);
            }
        }

        public byte[] TakePacket()
        {
            try
            {
                return packetCollection.Take();
            }
            catch (Exception)
            {

            }
            return null;
        }

        public void Stop()
        {
            if(udpClient != null)
            {
                try
                {
                    udpClient.Close();
                    udpClient.Dispose();
                    udpClient = null;
                }
                catch (Exception)
                {

                }
            }
        }
    }
}
