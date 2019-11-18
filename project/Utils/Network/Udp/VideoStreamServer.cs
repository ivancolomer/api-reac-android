using REAC_AndroidAPI.Utils.Network;
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
    class VideoStreamServer
    {
        //raspivid -n -vf -hf -ih -w 320 -h 240 -fps 24 -t 0 -o udp://192.168.1.154:8084

        public bool stopListeningTo;
        private UdpClient udpClient;

        private BlockingCollection<byte[]> packetCollection = new BlockingCollection<byte[]>(512);

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

                string ipReceiver = from.ToString().Split(':')[0];
                //string receiveString = Encoding.UTF8.GetString(receiveBytes);
                /*if(ipReceiver == "locking_device_ip")
                {

                }*/

                try
                {
                    packetCollection.TryAdd(receiveBytes);
                }
                catch(Exception)
                {

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
