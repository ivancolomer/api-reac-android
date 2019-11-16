using REAC_AndroidAPI.Utils.Loop;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace REAC_AndroidAPI.Utils.Network.Tcp.Sessions
{
    public class SessionManager
    {
        private const int LOOP_MILLS = 1 * 60 * 1000; //1min
        private const int MAX_TIME_DISCONNECTED = 5 * 60 * 1000; //5min

        private static ConcurrentDictionary<Session, byte> Sessions;
        private static InfiniteLoop SessionsChecker;

        public static void Initialize()
        {
            Sessions = new ConcurrentDictionary<Session, byte>();
            SessionsChecker = new InfiniteLoop(LOOP_MILLS, new OnTickCallback(CheckSessions));
        }

        private static void CheckSessions()
        {
            foreach (var session in Sessions)
            {
                if (Time.GetTime() - session.Key.LastTimeReaden >= MAX_TIME_DISCONNECTED)
                    session.Key.Close();
            }
        }

        public static void StopSession(Session Session)
        {
            byte trash;
            Sessions.TryRemove(Session, out trash);
        }

        public static void HandleIncomingConnection(Socket IncomingSocket)
        {
            Sessions.TryAdd(new Session(IncomingSocket), 0);
        }

        public static List<Session> CopySessions
        {
            get
            {
                return Sessions.Keys.ToList();
            }
        }
    }
}
