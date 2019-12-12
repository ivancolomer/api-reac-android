using REAC_AndroidAPI.Utils.Network.Tcp.Common;
using REAC_AndroidAPI.Utils.Output;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
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

        private ConcurrentDictionary<long, TaskCompletionSource<object>> MessagesWaitingForResponse;
        private long LastPacketID;

        private long imagePacketId = -1;
        private int imageSize = 0;
        private int imageRead = 0;
        private MemoryStream image = null;

        public LockerDevice(LockerDevicesManager clientManager, Socket Socket)
            : base(Socket)
        {
            ClientManager = clientManager;
            LastTimeReaden = Time.GetTime();
            MessagesWaitingForResponse = new ConcurrentDictionary<long, TaskCompletionSource<object>>();
            LastPacketID = 0;
        }

        public override void CleanUp()
        {
            ClientManager?.StopClient(this);
            Dispose();
        }

        public override void HandlePacket(byte[] body, int length)
        {
            int start = 0;
            LastTimeReaden = Time.GetTime();

            if (imagePacketId != -1)
            {
                int toRead = Math.Min(length, imageSize - imageRead);
                image.Write(body, 0, toRead);
                imageRead += toRead;

                
                if (imageRead < imageSize)
                {
                    return;
                }

                SendResponseToMessage(imagePacketId, image.ToArray());
                
                imagePacketId = -1;
                imageRead = 0;
                image = null;

                if(length - toRead == 0)
                    return;
                start = toRead;
            }

            string stringPacket = Encoding.UTF8.GetString(body, start, length - start);
            string totalHeader = "";

            string message;
            string value = getStringFirstValue(stringPacket, out message);
            if (value == null)
                return;
            totalHeader += value + "|";

            long packetId = long.Parse(value);

            if (message.StartsWith("error|"))
            {
                SendResponseToMessage(packetId, null);
                return;
            }

            if (message.StartsWith("send_image|"))
            {
                value = getStringFirstValue(message, out message);
                if (value == null)
                    return;
                totalHeader += value + "|";

                value = getStringFirstValue(message, out message);
                if (value == null)
                    return;
                totalHeader += value + "|";

                imageSize = int.Parse(value);
                if (imageSize > 0)
                {
                    image = new MemoryStream();
                    byte[] headerInBytes = Encoding.UTF8.GetBytes(totalHeader);
                    if (length - start > headerInBytes.Length)
                    {
                        image.Write(body, start + headerInBytes.Length, length - start - headerInBytes.Length);
                        imageRead += length - headerInBytes.Length - start;
                    }
                    imagePacketId = packetId;

                    return;
                }

                SendResponseToMessage(packetId, null);
                return;
            }

            SendResponseToMessage(packetId, message);
        }

        private void SendResponseToMessage(long packetId, object response)
        {
            TaskCompletionSource<object> tcs;
            if (MessagesWaitingForResponse.TryRemove(packetId, out tcs))
            {
                try
                {
                    tcs?.TrySetResult(response);
                }
                catch (Exception)
                {

                }
            }
        }

        public void BlockingSend(string Message, TaskCompletionSource<object> tcs)
        {
            long packetId = Interlocked.Increment(ref LastPacketID);

            if (tcs != null)
                MessagesWaitingForResponse.TryAdd(packetId, tcs);

            base.Send(packetId.ToString() + "|" + Message);

            if (tcs == null)
                return;

            var ct = new CancellationTokenSource(2000); //WAIT 2sec for response max
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

        public static string getStringFirstValue(string message, out string substring)
        {
            int separatorIndex = message.IndexOf('|');
            if (separatorIndex == -1)
            {
                substring = null;
                return null;
            }

            substring = message.Substring(separatorIndex + 1);
            return message.Substring(0, separatorIndex);
        }
    }
}
