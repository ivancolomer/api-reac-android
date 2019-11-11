﻿using REAC_AndroidAPI.Utils.Output;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace REAC2_AndroidAPI.Utils.Network.Udp
{
    class BroadcastReceiver
    {
        public bool stopListeningToBroadcast;
        private UdpClient udpClient;
        private IPEndPoint broadcastAddress;

        public BroadcastReceiver()
        {
            stopListeningToBroadcast = false;

            /*broadcastAddress = new IPEndPoint(IPAddress.Any, DotNetEnv.Env.GetInt("UDP_LISTENER_PORT"));
            udpClient = new UdpClient();
            udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, true);
            udpClient.ExclusiveAddressUse = false; // only if you want to send/receive on same machine.
            udpClient.Client.Bind(broadcastAddress);

            udpClient.BeginReceive(new AsyncCallback(ReceiveCallback), null);*/

            udpClient = new UdpClient(DotNetEnv.Env.GetInt("UDP_LISTENER_PORT"));
            udpClient.BeginReceive(new AsyncCallback(ReceiveCallback), null);
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                IPEndPoint from = new IPEndPoint(0, 0);
                byte[] receiveBytes = udpClient.EndReceive(ar, ref from);

                string ipReceiver = from.ToString().Split(':')[0];
                string receiveString = Encoding.UTF8.GetString(receiveBytes);

                //Logger.WriteLineWithHeader($"Received from {ipReceiver}: {receiveString}", "BROADCAST", Logger.LOG_LEVEL.DEBUG);

                if (!stopListeningToBroadcast)
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
                    catch(Exception e)
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
                Logger.WriteLine("Exception while ReceivedCallback from BroadcastReceiver: " + e.ToString(), Logger.LOG_LEVEL.ERROR);
            }
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
                catch (Exception e)
                {

                }
            }
        }
    }
}
