using REAC_AndroidAPI.Utils.Network.Tcp.Common;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace REAC_AndroidAPI.Utils.Network.Tcp.LockerDevices
{
    public class LockerDevice : Client
    {
        private const int TIME_OUT_MILLS = 5000;

        private LockerDevicesManager ClientManager;
        public long LastTimeReaden { get; set; }

        private ConcurrentDictionary<long, TaskCompletionSource<string>> MessagesWaitingForResponse;
        private long LastPacketID;

        public LockerDevice(LockerDevicesManager clientManager, Socket Socket)
            : base(Socket)
        {
            ClientManager = clientManager;
            LastTimeReaden = Time.GetTime();
            MessagesWaitingForResponse = new ConcurrentDictionary<long, TaskCompletionSource<string>>();
            LastPacketID = 0;
        }

        public override void CleanUp()
        {
            ClientManager?.StopClient(this);
            Dispose();
        }

        public override void HandlePacket(byte[] body)
        {
            LastTimeReaden = Time.GetTime();

            string stringPacket = Encoding.UTF8.GetString(body);
            //Used for talking with other devices from the same local network...
            int separatorIndex = stringPacket.IndexOf('|');
            if (separatorIndex == -1)
                return;

            TaskCompletionSource<string> tcs;
            if (MessagesWaitingForResponse.TryRemove(long.Parse(stringPacket.Substring(0, separatorIndex)), out tcs))
            {
                try
                {
                    tcs?.TrySetResult(stringPacket.Substring(separatorIndex + 1));
                }
                catch (Exception)
                {

                }
            }
        }

        public void BlockingSend(string Message, TaskCompletionSource<string> tcs)
        {
            long packetId = Interlocked.Increment(ref LastPacketID);

            if (tcs != null)
                MessagesWaitingForResponse.TryAdd(packetId, tcs);

            base.Send(packetId.ToString() + "|" + Message);

            if (tcs == null)
                return;

            var ct = new CancellationTokenSource(5000);
            ct.Token.Register(() =>
            {
                try
                {
                    if(MessagesWaitingForResponse.TryRemove(packetId, out _))
                        tcs?.TrySetCanceled();
                }
                catch (Exception)
                {
                }
            }, useSynchronizationContext: false);
        }

        public override void Dispose()
        {
            base.Dispose();
            ClientManager = null;
            foreach(var keyvalue in MessagesWaitingForResponse)
            {
                MessagesWaitingForResponse.Remove(keyvalue.Key, out _);
                try
                {
                    keyvalue.Value?.SetCanceled();
                }
                catch(Exception)
                {

                }
            }
        }
    }
}
