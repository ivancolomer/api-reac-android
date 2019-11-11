using Nancy;
using Nancy.Extensions;
using REAC_AndroidAPI.Entities;
using REAC_AndroidAPI.Exceptions;
using REAC_AndroidAPI.Handlers;
using REAC_AndroidAPI.Utils;
using REAC_AndroidAPI.Utils.Loop;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace REAC_AndroidAPI
{
    public class MainModule : NancyModule
    {
        private const int LOOP_MILLS = 1 * 60 * 1000; //1min
        private const int MAX_LIVE_TIME = 10 * 60 * 1000; //10min

        private ConcurrentDictionary<string, User> ConnectedUsers;
        private InfiniteLoop Looper;

        public MainModule()
            : base("api")
        {
            ConnectedUsers = new ConcurrentDictionary<string, User>();
            Looper = new InfiniteLoop(LOOP_MILLS, new OnTickCallback(CheckConnectedUsers));

#pragma warning disable CS1998

            Get("/", async (x, ct) =>
            {
                return Response.AsJson(new MainResponse<String>("Welcome to AndroidAPI"));
            });

            //LOGIN
            Get("/login", async (x, ct) =>
            {
                string userId = this.Request.Query["user_id"];
                string password = this.Request.Query["password"];

                if (userId == null || password == null)
                {
                    return Response.AsJson(new MainResponse<byte>(true, "Missing parameters in the GET request"));
                }

                //Check if the user and password are in the database
                if(false)
                {
                    return Response.AsJson(new MainResponse<byte>(true, "User or password incorrect"));
                }

                //If everything is good so far, update database with a new SessionID(GUID), save the IPAddress(UserHostAddress) and the LastTimeLogged
                //so we can ask for giving the password again if it was long since we haven't asked.

                string clientIP = this.Request.UserHostAddress;

                User newUser = new User
                {
                    IsOwner = false,
                    UserID = userId,
                    Name = "",
                    IPAddress = clientIP,
                    TimeCreated = Time.GetTime()
                };

                RemoveUserByUserID(userId);
                AddUser(newUser);

                //Send query to mysql if needed
                if (false)
                {
                    return Response.AsJson(new MainResponse<byte>(true, "Error while processing your request(1)"));
                }

                return Response.AsJson(new MainResponse<String>(newUser.SessionID));
            });

            //USER
            Post("/user", async (x, ct) =>
            {
                //string userId = this.Request.Query["user_id"];
                string sessionId = this.Request.Query["session_id"];

                if (sessionId == null) //userId == null || 
                {
                    return Response.AsJson(new MainResponse<byte>(true, "missing_request_parameters"));
                }

                if(!CheckLogIn(sessionId)) //userId, 
                    return Response.AsJson(new MainResponse<byte>(true, "expired_session_id"));

                return Response.AsJson(new MainResponse<String>("Something"));
            });

            Get("/user", async (x, ct) =>
            {
                await Task.Delay(1000);
                var jsonString = this.Request.Body.AsString();
                return Response.AsJson(jsonString);
                /*return Response.AsJson(new List<User>() {
                    new User(userId),
                });*/
            });

            Put("/user", async (x, ct) =>
            {
                await Task.Delay(1000);
                var jsonString = this.Request.Body.AsString();
                return Response.AsJson(jsonString);
                /*return Response.AsJson(new List<User>() {
                    new User(userId),
                });*/
            });

#pragma warning restore CS1998
        }

        private void CheckConnectedUsers()
        {
            foreach (var user in ConnectedUsers)
            {
                if (Time.GetTime() - user.Value.TimeCreated >= MAX_LIVE_TIME)
                    ConnectedUsers.TryRemove(user.Key, out _);
            }
        }

        private void AddUser(User user)
        {
            do
            {
                Guid guid = Guid.NewGuid();
                var guidBytes = guid.ToByteArray();
                user.SessionID = BitConverter.ToString(guidBytes);
            }
            while (!ConnectedUsers.TryAdd(user.SessionID, user));
        }

        private void RemoveUserByUserID(string userId)
        {
            foreach(var keyvalue in ConnectedUsers)
            {
                if(keyvalue.Value.UserID == userId)
                {
                    ConnectedUsers.TryRemove(keyvalue.Key, out _);
                }
            }
        }

        private bool CheckLogIn(string sessionId) //string userId
        {
            User user;
            return ConnectedUsers.TryGetValue(sessionId, out user); //&& user.UserID == userId;
        }
    }
}
