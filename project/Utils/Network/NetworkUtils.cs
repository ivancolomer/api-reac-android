using REAC_AndroidAPI.Utils.Output;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace REAC_AndroidAPI.Utils.Network
{
    class NetworkUtils
    {
        private static string ExternalIPAddress = null;

        public static string GetExternalIPAddress()
        {
            if(ExternalIPAddress == null)
            {
                try
                {
                    ExternalIPAddress = new WebClient().DownloadString("https://api.ipify.org");
                }
                catch (Exception e)
                {
                    Logger.WriteLine("Errror while getting IPAddress: " + e.ToString(), Logger.LOG_LEVEL.ERROR);
                }
            }

            return ExternalIPAddress;

            /*string localIP = null;
            try
            {

                using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
                {
                    socket.Connect("8.8.8.8", 65530);
                    IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
                    localIP = endPoint.Address.ToString();
                }

            }
            catch (Exception e)
            {
                Logger.WriteLine("Errror while getting IPAddress: " + e.ToString(), Logger.LOG_LEVEL.ERROR);
            }
            return localIP;*/
        }
    }
}
