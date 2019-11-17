using REAC_AndroidAPI.Entities;
using REAC_AndroidAPI.Utils;
using REAC_AndroidAPI.Utils.Loop;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace REAC_AndroidAPI.Handlers.Requests
{
    public class UsersManager
    {
        private const int LOOP_MILLS = 1 * 60 * 1000; //1min
        private const int MAX_LIVE_TIME = 10 * 60 * 1000; //10min

        private static ConcurrentDictionary<string, LocalUser> ConnectedUsers;
        private static InfiniteLoop Looper;

        public static void Initialize()
        {
            ConnectedUsers = new ConcurrentDictionary<string, LocalUser>();
            Looper = new InfiniteLoop(LOOP_MILLS, new OnTickCallback(CheckConnectedUsers));
        }

        public static void CheckConnectedUsers()
        {
            foreach (var user in ConnectedUsers)
            {
                if (Time.GetTime() - user.Value.TimeCreated >= MAX_LIVE_TIME)
                {
                    ConnectedUsers.TryRemove(user.Key, out _);
                    //Logger.WriteLine("DISCONNECTED: " + user.Key, Logger.LOG_LEVEL.DEBUG);
                }
            }
        }

        public static void AddUser(LocalUser user)
        {
            do
            {
                user.SessionID = BitConverter.ToString(Guid.NewGuid().ToByteArray());
            }
            while (!ConnectedUsers.TryAdd(user.SessionID, user));
        }

        public static void RemoveUserByUserName(string userName)
        {
            foreach (var keyvalue in ConnectedUsers)
            {
                if (keyvalue.Value.Name == userName)
                {
                    ConnectedUsers.TryRemove(keyvalue.Key, out _);
                }
            }
        }

        public static bool CheckLogIn(string sessionId, string ipAddress, out LocalUser user) //string userId
        {
            /*foreach (var kvp in ConnectedUsers)
            {
                Logger.WriteLine("Key = " + kvp.Key + ", Value = " + kvp.Value.Name, Logger.LOG_LEVEL.DEBUG);
            }*/

            return ConnectedUsers.TryGetValue(sessionId, out user) && user.IPAddress == ipAddress;
        }
    }
}
