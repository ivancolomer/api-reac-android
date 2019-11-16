using Nancy;
using Nancy.Extensions;
using Newtonsoft.Json.Linq;
using REAC_AndroidAPI.Handlers;
using REAC_AndroidAPI.Entities;
using REAC_AndroidAPI.Utils;
using REAC_AndroidAPI.Utils.Loop;
using REAC_AndroidAPI.Utils.Output;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using REAC_AndroidAPI.Utils.Network;

namespace REAC_AndroidAPI.Handlers.Requests
{
    public class RequestHandler : NancyModule
    {
        

        public RequestHandler()
            : base("api")
        {
#pragma warning disable CS1998

            Get("/", async (x, ct) =>
            {
                return Response.AsJson(new MainResponse<String>("Welcome to AndroidAPI"));
            });

            Get("/ipaddress", async (x, ct) =>
            {
                string ipAddress = NetworkUtils.GetExternalIPAddress();
                if(ipAddress == null)
                    return Response.AsJson(new MainResponse<byte>(true, "unable_get_ipaddress"));

                return Response.AsJson(new MainResponse<String>(ipAddress));
            });

            //LOGIN
            Get("/login", async (x, ct) =>
            {
                string userName = this.Request.Query["user_name"];
                string password = this.Request.Query["password"];
                //uint userId;

                if (userName == null || password == null)//if (userIdString == null || password == null || !uint.TryParse(userIdString, out userId))
                {
                    return Response.AsJson(new MainResponse<byte>(true, "missing_request_parameters"));
                }

                if (userName.Length < 4)
                    return Response.AsJson(new MainResponse<byte>(true, "short_username_length"));

                //Check if the user and password are in the database
                User user = User.GetAdministratorFromDB(userName, password);
                if (user == null)
                {
                    return Response.AsJson(new MainResponse<byte>(true, "wrong_user_password"));
                }

                //If everything is good so far, update database with a new SessionID(GUID), save the IPAddress(UserHostAddress) and the LastTimeLogged
                //so we can ask for giving the password again if it was long since we haven't asked.

                user.IPAddress = this.Request.UserHostAddress;
                user.TimeCreated = Time.GetTime();

                UsersManager.RemoveUserByUserName(userName);
                UsersManager.AddUser(user);

                return Response.AsJson(new MainResponse<String>(user.SessionID));
            });

            //REGISTER FOR FIRST TIME
            Post("/register", async (x, ct) =>
            {
                var bodyJson = JObject.Parse(this.Request.Body.AsString());

                string serialId = bodyJson["serial_id"]?.ToString();
                string userName = bodyJson["user_name"]?.ToString();

                if (serialId == null || userName == null)
                {
                    return Response.AsJson(new MainResponse<byte>(true, "missing_request_parameters"));
                }

                if(userName.Length < 4)
                    return Response.AsJson(new MainResponse<byte>(true, "short_username_length"));

                int adminCount = User.GetAdministratorsCountFromDB();

                if(adminCount == -1)
                    return Response.AsJson(new MainResponse<byte>(true, "database_error"));

                if (adminCount >= 1)
                    return Response.AsJson(new MainResponse<byte>(true, "admin_already_exists"));

                if(serialId != DotNetEnv.Env.GetString("SERIAL_ID"))
                    return Response.AsJson(new MainResponse<byte>(true, "wrong_serial_id"));

                //INSERT NEW ADMINISTRATOR ACCOUNT, return random password...
                byte[] newPasswordBytes = RandomGenerator.GenerateRandomBytes(32);

                int status = User.InsertNewAdministratorToDB(new User
                {
                    Name = userName,
                    Role = "ADMIN"
                }, newPasswordBytes);

                if (status == 1)
                    return Response.AsJson(new MainResponse<String>(Convert.ToBase64String(newPasswordBytes)));
                else if(status == -2)
                    return Response.AsJson(new MainResponse<byte>(true, "name_already_in_use"));
                else if (status == -3)
                    return Response.AsJson(new MainResponse<byte>(true, "member_is_already_an_admin"));
                else
                    return Response.AsJson(new MainResponse<byte>(true, "database_error"));
            });

            //USER
            Post("/user", async (x, ct) =>
            {
                string sessionId = this.Request.Query["session_id"];

                if (sessionId == null)
                {
                    return Response.AsJson(new MainResponse<byte>(true, "missing_request_parameters"));
                }

                User user;
                if(!UsersManager.CheckLogIn(sessionId, this.Request.UserHostAddress, out user)) 
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
    }
}
