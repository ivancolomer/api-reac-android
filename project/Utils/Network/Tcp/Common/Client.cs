using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace REAC_AndroidAPI.Utils.Network.Tcp.Common
{
    public abstract class Client : IDisposable
    {
        private const int BUFFER_LENGTH = 4096;

        private byte[] Buffer;
        private Socket Socket;

        private readonly object CleanUpLock;
        private bool CleanedUp;

        /// <summary>
        /// State of the connection.
        /// </summary>
        public ConnectionState State { get; private set; }

        /// <summary>
        /// Remote address.
        /// </summary>
        public string Address { get; private set; }

        private readonly object lockSync;

        /// <summary>
        /// Creates new connection.
        /// </summary>
        public Client(Socket socket)
        {
            this.CleanUpLock = new object();
            this.lockSync = new object();
            this.Buffer = new byte[BUFFER_LENGTH];
            this.Socket = socket;
            this.State = ConnectionState.Open;
            this.Address = ((IPEndPoint)socket.RemoteEndPoint).ToString().Split(':')[0];

            this.BeginReceive();
        }

        /// <summary>
        /// Closes the connection.
        /// </summary>
        public void Close()
        {
            lock (lockSync)
            {
                if (this.State == ConnectionState.Closed)
                {
                    //Log.Warning("Attempted closing of an already closed connection.");
                    return;
                }

                this.State = ConnectionState.Closed;
            }
            try { Socket.Close(); }
            catch(Exception) { }

            this.OnClosed();

            //Log.Info("Closed connection from '{0}'.", this.Address);
        }

        /// <summary>
        /// Starts packet receiving.
        /// </summary>
        public void BeginReceive()
        {
            Socket.BeginReceive(Buffer, 0, Buffer.Length, SocketFlags.None, OnReceive, null);
        }

        /// <summary>
        /// Called when new data is available from socket.
        /// </summary>
        /// <param name="result"></param>
        private void OnReceive(IAsyncResult result)
        {
            try
            {
                var length = Socket.EndReceive(result);

                // Client disconnected
                if (length == 0)
                {
                    this.Close();
                    return;
                }

                // Handle
                try
                {
                    this.HandlePacket(Buffer);
                }
                catch (Exception)
                {
                    //Log.Exception(ex, "Error while handling packet.");
                }

                this.BeginReceive();
            }
            catch (SocketException)
            {
                this.Close();
                //Log.Info("Lost connection from '{0}'.", this.Address);

            }
            catch (ObjectDisposedException)
            {
            }
            catch (Exception)
            {
                //Log.Exception(ex, "Error while receiving packet.");
            }
        }

        public void Send(string Message)
        {
            Send(Encoding.UTF8.GetBytes(Message));
        }

        public virtual void Send(byte[] Data)
        {
            try
            {
                ConnectionState getState;
                lock (lockSync)
                {
                    getState = State;
                }
                if (getState == ConnectionState.Open && Socket != null && Socket.Connected)
                {
                    Socket.BeginSend(Data, 0, Data.Length, SocketFlags.None, new AsyncCallback(OnDataSent), null);
                }
            }
            catch (SocketException)
            {
                this.Close();
            }
            catch (ObjectDisposedException)
            {
            }
            catch (Exception)
            {

            }
        }

        private void OnDataSent(IAsyncResult Result)
        {
            try
            {
                if (Socket != null)
                {
                    Socket.EndSend(Result);
                }
            }
            catch (SocketException)
            {
                this.Close();
            }
            catch (ObjectDisposedException)
            {

            }
        }

        /// <summary>
        /// To be called when connection is closed, calls event
        /// and CleanUp.
        /// </summary>
        private void OnClosed()
        {
            lock (CleanUpLock)
            {
                if (!CleanedUp)
                    this.CleanUp();

                CleanedUp = true;
            }
        }

        /// <summary>
        /// Called when the connection is closed.
        /// </summary>
        public abstract void CleanUp();

        /// <summary>
        /// Called for every packet that is read from the network stream.
        /// </summary>
        public abstract void HandlePacket(byte[] Body);

        /// <summary>
        /// Called when Session is Disposed.
        /// </summary>
        public virtual void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }

    public enum ConnectionState
    {
        Closed,
        Open,
    }
}
