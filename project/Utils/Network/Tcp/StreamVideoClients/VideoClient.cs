using REAC_AndroidAPI.Utils.Network.Tcp.Common;
using REAC_AndroidAPI.Utils.Output;
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
            Logger.WriteLineWithHeader("CLOSED CONNECTION", "VIDEO - " + Address, Logger.LOG_LEVEL.WARN);
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
            //Logger.WriteLineWithHeader(ByteArrayToHexViaLookup32(Data), "VIDEO - " + Address, Logger.LOG_LEVEL.WARN);
            Logger.WriteLineWithHeader("Sent " + Data.Length + " bytes", "VIDEO - " + Address, Logger.LOG_LEVEL.WARN);
        }

        public override void Dispose()
        {
            base.Dispose();
        }

        private static readonly uint[] _lookup32 = CreateLookup32();

        private static uint[] CreateLookup32()
        {
            var result = new uint[256];
            for (int i = 0; i < 256; i++)
            {
                string s = i.ToString("X2");
                result[i] = ((uint)s[0]) + ((uint)s[1] << 16);
            }
            return result;
        }

        private static string ByteArrayToHexViaLookup32(byte[] bytes)
        {
            var lookup32 = _lookup32;
            var result = new char[bytes.Length * 2];
            for (int i = 0; i < bytes.Length; i++)
            {
                var val = lookup32[bytes[i]];
                result[2 * i] = (char)val;
                result[2 * i + 1] = (char)(val >> 16);
            }
            return new string(result);
        }
    }
}
