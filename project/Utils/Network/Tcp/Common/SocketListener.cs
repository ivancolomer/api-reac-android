using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace REAC_AndroidAPI.Utils.Network.Tcp.Common
{
   public delegate void OnNewConnectionCallback(Socket Socket);

    public class SocketListener : IDisposable
    {
        private Socket mSocket;
        private OnNewConnectionCallback mCallback;

        public SocketListener(IPEndPoint LocalEndpoint, int Backlog, OnNewConnectionCallback Callback)
        {
            mCallback = Callback;

            mSocket = new Socket(LocalEndpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            if(!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                mSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, 1);

            mSocket.Bind(LocalEndpoint);
            mSocket.Listen(Backlog);
            mSocket.Blocking = false;

            BeginAccept();
        }

        public void Dispose()
        {
            if (mSocket != null)
            {
                mSocket.Close();
                mSocket.Dispose();
                mSocket = null;
            }
        }

        private void BeginAccept()
        {
            try
            {
                mSocket.BeginAccept(OnAccept, null);
            }
            catch (Exception) { }
        }

        private void OnAccept(IAsyncResult Result)
        {
            try
            {
                Socket ResultSocket = (Socket)mSocket.EndAccept(Result);
                mCallback.Invoke(ResultSocket);
            }
            catch (Exception) { }

            BeginAccept();
        }
    }
}
