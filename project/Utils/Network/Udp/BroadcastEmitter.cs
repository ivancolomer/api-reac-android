using REAC_AndroidAPI.Utils.Loop;
using REAC_AndroidAPI.Utils.Output;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;

namespace REAC2_AndroidAPI.Utils.Network.Udp
{
    public class BroadcastEmitter
    {
        private const int LOOP_MILLS = 3 * 1000; // 3 seconds time

        private InfiniteLoop BroadcastLoop;
        private UdpClient UdpClient;
        private IPEndPoint UdpAddress;

        public BroadcastEmitter()
        {
            UdpAddress = new IPEndPoint(IPAddress.Broadcast, DotNetEnv.Env.GetInt("UDP_LISTENER_PORT"));
            UdpClient = new UdpClient();
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                UdpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, 1);
            UdpClient.EnableBroadcast = true;

            BroadcastLoop = new InfiniteLoop(LOOP_MILLS, new OnTickCallback(SendBroadcastMessage));
        }

        private void SendBroadcastMessage()
        {
            try
            {
                if(UdpClient != null && UdpAddress != null) //Check if UDP
                {
                    byte[] bytes = Encoding.UTF8.GetBytes("REAC"); // UTF-8 String to array of bytes
                    UdpClient.Send(bytes, bytes.Length, UdpAddress); // Send the array of bytes
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
                Logger.WriteLine("Exception->BroadcastEmiter::SendBroadcastMessage " + e.ToString(), Logger.LOG_LEVEL.ERROR);
            }
        }

        public void Stop()
        {
            try
            {
                BroadcastLoop.Stop();
                BroadcastLoop = null;

                UdpClient.Close();
                UdpClient.Dispose();
                UdpClient = null;

                UdpAddress = null;
            }
            catch (Exception e)
            {
                Logger.WriteLine("Exception->BroadcastEmiter::Stop " + e.ToString(), Logger.LOG_LEVEL.ERROR);
            }
        }
    }
}
