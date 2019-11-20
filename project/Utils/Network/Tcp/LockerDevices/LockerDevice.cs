using REAC_AndroidAPI.Utils.Network.Tcp.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace REAC_AndroidAPI.Utils.Network.Tcp.LockerDevices
{
    public class LockerDevice : Client
    {
        private LockerDevicesManager ClientManager;
        public long LastTimeReaden { get; set; }

        public LockerDevice(LockerDevicesManager clientManager, Socket Socket)
            : base(Socket)
        {
            ClientManager = clientManager;
            LastTimeReaden = Time.GetTime();
        }

        public void TryAuthenticate(int userId, ulong token)
        {
            
        }

        public override void CleanUp()
        {
            ClientManager.StopClient(this);
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

        public override void Dispose()
        {
            base.Dispose();
        }
    }
}
