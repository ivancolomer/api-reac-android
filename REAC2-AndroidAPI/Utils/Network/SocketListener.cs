using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace REAC_AndroidApi.Utils.Network
{
    public class SocketOption
    {
        private SocketOptionLevel optionLevel;
        private SocketOptionName optionName;
        private int optionValue;

        public SocketOption(SocketOptionLevel optionLevel, SocketOptionName optionName, int optionValue)
        {
            this.optionLevel = optionLevel;
            this.optionName = optionName;
            this.optionValue = optionValue;
        }

        public SocketOptionLevel getOptionLevel()
        {
            return optionLevel;
        }

        public SocketOptionName getOptionName()
        {
            return optionName;
        }

        public int getOptionValue()
        {
            return optionValue;
        }

    }

    public delegate void OnNewConnectionCallback(Socket Socket);

    public class SocketListener : IDisposable
    {
        private Socket mSocket;
        private OnNewConnectionCallback mCallback;

        public SocketListener(IPEndPoint LocalEndpoint, int Backlog, OnNewConnectionCallback Callback, SocketOption option = null)
        {
            mCallback = Callback;

            mSocket = new Socket(LocalEndpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            if (option != null)
                mSocket.SetSocketOption(option.getOptionLevel(), option.getOptionName(), option.getOptionValue());

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
