using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace REAC_AndroidAPI.Utils.Network.Tcp.Sessions
{
    public class Session : Client
    {
        public long LastTimeReaden { get; set; }

        public Session(Socket Socket)
            : base(Socket)
        {
            LastTimeReaden = Time.GetTime();
        }

        public void TryAuthenticate(int userId, ulong token)
        {
            
        }

        public override void CleanUp()
        {
            SessionManager.StopSession(this);

            Dispose();
        }

        public override void HandlePacket(byte[] body)
        {
            LastTimeReaden = Time.GetTime();

            string stringPacket = Encoding.UTF8.GetString(body);
            //Used for talking with other devices from the same local network...
            
        }

        public override void Send(byte[] Data)
        {
            base.Send(Data);
        }

        public void Send(string Message)
        {
            Send(Encoding.UTF8.GetBytes(Message + (char)0x00));
        }

        public override void Dispose()
        {
            base.Dispose();
        }
    }
}
