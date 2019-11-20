using REAC_AndroidAPI.Utils.Network.Tcp.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace REAC_AndroidAPI.Utils.Network.Tcp.StreamVideoClients
{
    public class VideoClient : Client
    {
        private VideoClientsManager ClientManager;
        public long LastTimeWritten { get; set; }

        public VideoClient(VideoClientsManager clientManager, Socket Socket)
            : base(Socket)
        {
            ClientManager = clientManager;
            LastTimeWritten = Time.GetTime();
        }

        public override void CleanUp()
        {
            ClientManager.StopClient(this);
            Dispose();
        }

        public override void HandlePacket(byte[] body)
        {
            string stringPacket = Encoding.UTF8.GetString(body);
            //Used for talking with other devices from the same local network...
        }

        public override void Send(byte[] Data)
        {
            LastTimeWritten = Time.GetTime();
            base.Send(Data);
        }

        public override void Dispose()
        {
            base.Dispose();
        }
    }
}
