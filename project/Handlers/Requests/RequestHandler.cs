using Nancy;
using Nancy.Extensions;
using Newtonsoft.Json.Linq;
using REAC_AndroidAPI.Entities;
using REAC_AndroidAPI.Utils;
using System;
using System.Threading.Tasks;
using REAC_AndroidAPI.Utils.Network;
using System.Collections.Generic;
using REAC_AndroidAPI.Utils.Responses;
using REAC_AndroidAPI.Utils.Storage;

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
                LocalUser user = LocalUser.GetAdministratorFromDB(userName, password);
                if (user == null)
                {
                    return Response.AsJson(new MainResponse<byte>(true, "wrong_user_password"));
                }

                //If everything is good so far, update database with a new SessionID(GUID), save the IPAddress(UserHostAddress) and the LastTimeLogged
                //so we can ask for giving the password again if it was long since we haven't asked.

                user.IPAddress = this.Request.UserHostAddress.ToString().Split(':')[0];
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

                int adminCount = LocalUser.GetAdministratorsCountFromDB();

                if(adminCount == -1)
                    return Response.AsJson(new MainResponse<byte>(true, "database_error"));

                if (adminCount >= 1)
                    return Response.AsJson(new MainResponse<byte>(true, "admin_already_exists"));

                if(serialId != DotNetEnv.Env.GetString("SERIAL_ID"))
                    return Response.AsJson(new MainResponse<byte>(true, "wrong_serial_id"));

                //INSERT NEW ADMINISTRATOR ACCOUNT, return random password...
                byte[] newPasswordBytes = RandomGenerator.GenerateRandomBytes(32);

                long status = LocalUser.InsertNewAdministratorToDB(new LocalUser
                {
                    Name = userName,
                    Role = "ADMIN"
                }, newPasswordBytes);

                if (status > 0)
                    return Response.AsJson(new MainResponse<String>(Convert.ToBase64String(newPasswordBytes)));
                else if(status == -2)
                    return Response.AsJson(new MainResponse<byte>(true, "name_already_in_use"));
                else if (status == -3)
                    return Response.AsJson(new MainResponse<byte>(true, "member_is_already_an_admin"));
                else
                    return Response.AsJson(new MainResponse<byte>(true, "database_error"));
            });

            //CREATE NEW ADMIN FROM USER
            Post("/admin", async (x, ct) =>
            {
                var bodyJson = JObject.Parse(this.Request.Body.AsString());

                string sessionId = bodyJson["session_id"]?.ToString();
                string userIdString = bodyJson["user_id"]?.ToString();
                uint userId = 0;

                if (sessionId == null || userIdString == null || !uint.TryParse(userIdString, out userId))
                {
                    return Response.AsJson(new MainResponse<byte>(true, "missing_request_parameters"));
                }

                LocalUser user;
                if(!UsersManager.CheckLogIn(sessionId, this.Request.UserHostAddress, out user)) 
                    return Response.AsJson(new MainResponse<byte>(true, "expired_session_id"));

                User member = null;

                int status = LocalUser.GetUserFromDB(userId, out member);
                if(status == 0 && member.IsOwner)
                    return Response.AsJson(new MainResponse<byte>(true, "member_is_already_an_admin"));
                if(status == -1)
                    return Response.AsJson(new MainResponse<byte>(true, "database_error"));

                LocalUser newUser = new LocalUser()
                {
                    IsOwner = false,
                    UserID = member.UserID,
                    ProfilePhoto = member.ProfilePhoto,
                    Name = member.Name,
                    Role = member.Role,
                    TimeCreated = Time.GetTime() - UsersManager.MAX_LIVE_TIME + 5 * 60 * 1000, //5min live time
                };

                UsersManager.AddUser(newUser);
                return Response.AsJson(new MainResponse<String>(newUser.SessionID));
            });

            Get("/admin/confirm", async (x, ct) =>
            {
                string sessionId = this.Request.Query["session_id"];

                if (sessionId == null)
                {
                    return Response.AsJson(new MainResponse<byte>(true, "missing_request_parameters"));
                }

                LocalUser user;
                if (!UsersManager.CheckSignUp(sessionId, out user))
                    return Response.AsJson(new MainResponse<byte>(true, "expired_session_id"));

                byte[] newPasswordBytes = RandomGenerator.GenerateRandomBytes(32);

                int status = LocalUser.InsertNewAdministratorToDBFromExistingUser(user, newPasswordBytes);

                if (status == 1)
                {
                    user.IsOwner = true;
                    return Response.AsJson(new MainResponse<String>(Convert.ToBase64String(newPasswordBytes)));
                }
                else if (status == -3)
                    return Response.AsJson(new MainResponse<byte>(true, "member_is_already_an_admin"));
                else if (status == -4)
                    return Response.AsJson(new MainResponse<byte>(true, "member_id_not_found"));
                else
                    return Response.AsJson(new MainResponse<byte>(true, "database_error"));

            });

            Post("/user", async (x, ct) =>
            {
                var bodyJson = JObject.Parse(this.Request.Body.AsString());

                string sessionId = bodyJson["session_id"]?.ToString();
                string userName = bodyJson["user_name"]?.ToString();
                string userRole = bodyJson["user_role"]?.ToString();

                if (sessionId == null || userName == null || userRole == null)
                {
                    return Response.AsJson(new MainResponse<byte>(true, "missing_request_parameters"));
                }

                if (userName.Length < 4)
                    return Response.AsJson(new MainResponse<byte>(true, "short_username_length"));

                LocalUser user;
                if (!UsersManager.CheckLogIn(sessionId, this.Request.UserHostAddress, out user))
                    return Response.AsJson(new MainResponse<byte>(true, "expired_session_id"));

                long userId = LocalUser.InsertNewMemberToDB(new User()
                {
                    Name = userName,
                    IsOwner = false,
                    Role = userRole
                });

                if(userId > 0)
                    return Response.AsJson(new MainResponse<long>(userId));
                if(userId == -2)
                    return Response.AsJson(new MainResponse<byte>(true, "name_already_in_use"));
                return Response.AsJson(new MainResponse<byte>(true, "database_error"));
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

            Get("/user/image/{id}", async (x, ct) =>
            {
                uint id = 0;
                string sessionId = this.Request.Query["session_id"];
                
                if (sessionId == null || !UInt32.TryParse(x.id.ToString(), out id))
                {
                    return Response.AsJson(new MainResponse<byte>(true, "missing_request_parameters"));
                }

                LocalUser user;
                if (!UsersManager.CheckLogIn(sessionId, this.Request.UserHostAddress, out user))
                    return Response.AsJson(new MainResponse<byte>(true, "expired_session_id"));

                return Response.FromByteArray(ProfilePhoto.GetProfilePhoto(id), "image/jpeg");
            });

            Get("/user/{id}/images", async (x, ct) =>
            {
                string sessionId = this.Request.Query["session_id"];
                string userIdString = x.id.ToString();
                uint userId = 0;

                if (sessionId == null || userIdString == null || !UInt32.TryParse(userIdString, out userId))
                {
                    return Response.AsJson(new MainResponse<byte>(true, "missing_request_parameters"));
                }

                LocalUser user;
                if (!UsersManager.CheckLogIn(sessionId, this.Request.UserHostAddress, out user))
                    return Response.AsJson(new MainResponse<byte>(true, "expired_session_id"));

                List<String> images;
                int status = LocalUser.GetImagesByUserFromDB(userId, out images);

                if (status == -1)
                    return Response.AsJson(new MainResponse<byte>(true, "database_error"));
                /*else if(status == -1)
                    return Response.AsJson(new MainResponse<byte>(true, "wrong_username"));*/

                return Response.AsJson(new MainResponse<List<String>>(images));
            });

            //USERS
            Get("/users", async (x, ct) =>
            {
                string sessionId = this.Request.Query["session_id"];
                if (sessionId == null)
                {
                    return Response.AsJson(new MainResponse<byte>(true, "missing_request_parameters"));
                }

                LocalUser user;
                if (!UsersManager.CheckLogIn(sessionId, this.Request.UserHostAddress, out user))
                    return Response.AsJson(new MainResponse<byte>(true, "expired_session_id"));

                List<User> users;
                int status = LocalUser.GetUsersFromDB(out users);

                if(status < 0)
                    return Response.AsJson(new MainResponse<byte>(true, "database_error"));

                return Response.AsJson(new MainResponse<List<User>>(users));
            });

            Get("/video", async (x, ct) =>
            {
                string sessionId = this.Request.Query["session_id"];
                if (sessionId == null)
                {
                    return Response.AsJson(new MainResponse<byte>(true, "missing_request_parameters"));
                }

                LocalUser user;
                if (!UsersManager.CheckLogIn(sessionId, this.Request.UserHostAddress, out user))
                    return Response.AsJson(new MainResponse<byte>(true, "expired_session_id"));

                string ipAddress = NetworkUtils.GetExternalIPAddress();
                if (ipAddress == null)
                    return Response.AsJson(new MainResponse<byte>(true, "unable_get_ipaddress"));

                if(Program.VideoClientsManager.AddIPAddress(user.IPAddress) && Time.GetTime() - user.TimeCreated >= 5 * 60 * 1000)
                {
                    user.TimeCreated += 5 * 60 * 1000;
                }

                //tcp/h264://192.168.1.154:8082
                return Response.AsJson(new MainResponse<String>("tcp/h264://" + ipAddress + ":" + DotNetEnv.Env.GetInt("TCP_VIDEO_LISTENER_PORT").ToString()));
            });

            Get("/door", async (x, ct) =>
            {
                string sessionId = this.Request.Query["session_id"];
                if (sessionId == null)
                {
                    return Response.AsJson(new MainResponse<byte>(true, "missing_request_parameters"));
                }

                LocalUser user;
                if (!UsersManager.CheckLogIn(sessionId, this.Request.UserHostAddress, out user))
                    return Response.AsJson(new MainResponse<byte>(true, "expired_session_id"));

                List<string> responses = null;
                try
                {
                    responses = await Program.LockerDevicesManager.SendMessageToAllDevicesBlocking("open_door");
                }
                catch(Exception)
                {

                }

                if(responses != null && responses.Count > 0)
                    return Response.AsJson(new MainResponse<List<string>>(responses));

                return Response.AsJson(new MainResponse<byte>(true, "locker_device_not_found"));
            });

#pragma warning restore CS1998
        }
    }
}
