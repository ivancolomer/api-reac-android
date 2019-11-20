using REAC_AndroidAPI.Utils.Loop;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace REAC_AndroidAPI.Utils.Network.Tcp.Common
{
    public abstract class ClientManager
    {
        public ConcurrentDictionary<Client, byte> Clients;
        private InfiniteLoop ClientsChecker;

        public SocketListener ServerListener;

        public ClientManager(int loopMills)
        {
            this.Clients = new ConcurrentDictionary<Client, byte>();
            this.ClientsChecker = new InfiniteLoop(loopMills, new OnTickCallback(CheckSessions));

            this.ServerListener = CreateSocketListener();
        }

        public abstract void CheckSessions();

        public virtual void StopClient(Client client)
        {
            byte trash;
            Clients.TryRemove(client, out trash);
        }

        public abstract SocketListener CreateSocketListener();

        public abstract void HandleIncomingConnection(Socket incomingSocket);

        public List<Client> CopySessions
        {
            get
            {
                return Clients.Keys.ToList();
            }
        }
    }
}
