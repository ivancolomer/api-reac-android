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
using System.Linq;
using System.IO;
using System.Globalization;

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

            //IPADDRESS
            Get("/ipaddress", async (x, ct) =>
            {
                string ipAddress = NetworkUtils.GetExternalIPAddress();
                if(ipAddress == null)
                    return Response.AsJson(new MainResponse<byte>(true, "unable_get_ipaddress"));

                return Response.AsJson(new MainResponse<String>(ipAddress));
            });

            //REGISTER
            Post("/register", async (x, ct) =>
            {
                var bodyJson = JObject.Parse(this.Request.Body.AsString());

                string serialId = bodyJson["serial_id"]?.ToString();
                string userName = bodyJson["user_name"]?.ToString();

                if (serialId == null || userName == null)
                {
                    return Response.AsJson(new MainResponse<byte>(true, "missing_request_parameters"));
                }

                if (userName.Length < 4)
                    return Response.AsJson(new MainResponse<byte>(true, "short_username_length"));

                int adminCount = LocalUser.GetAdministratorsCountFromDB();

                if (adminCount == -1)
                    return Response.AsJson(new MainResponse<byte>(true, "database_error"));

                if (adminCount >= 1)
                    return Response.AsJson(new MainResponse<byte>(true, "admin_already_exists"));

                if (serialId != DotNetEnv.Env.GetString("SERIAL_ID"))
                    return Response.AsJson(new MainResponse<byte>(true, "wrong_serial_id"));

                //INSERT NEW ADMINISTRATOR ACCOUNT, return random password...
                byte[] newPasswordBytes = RandomGenerator.GenerateRandomBytes(32);

                long status = LocalUser.InsertNewAdministratorToDB(new LocalUser
                {
                    Name = userName,
                    Role = "ADMIN"
                }, newPasswordBytes);

                if (status > 0)
                    return Response.AsJson(new MainResponse<RegisterResponse>(new RegisterResponse()
                    {
                        Password = Convert.ToBase64String(newPasswordBytes),
                        Id = status
                    }));
                else if (status == -2)
                    return Response.AsJson(new MainResponse<byte>(true, "name_already_in_use"));
                else if (status == -3)
                    return Response.AsJson(new MainResponse<byte>(true, "member_is_already_an_admin"));
                else
                    return Response.AsJson(new MainResponse<byte>(true, "database_error"));
            });

            /* 
             * Login Administrator GET Request.
             * Returns a String on content field with the new generated random session_id.
             */
            Get("/login", async (x, ct) =>
            {
                string userName = this.Request.Query["user_name"];
                string password = this.Request.Query["password"];

                if (userName == null || password == null)
                    return Response.AsJson(new MainResponse<byte>(true, "missing_request_parameters"));               

                if (userName.Length < 4)
                    return Response.AsJson(new MainResponse<byte>(true, "short_username_length"));

                /* 
                 * Check if the user and password are in the database and, if it's not there, 
                 * return an error message
                 */
                LocalUser user = LocalUser.GetAdministratorFromDB(userName, password);
                if (user == null)
                    return Response.AsJson(new MainResponse<byte>(true, "wrong_user_password"));

                /*
                 * If everything is good so far:
                 *   - Save the IPAddress(UserHostAddress) and the last time the administrator has logged in.
                 *   - Remove, if there's any, the previous active session from the same user.
                 *   - Create a new SessionID(GUID) and save the user into UsersManager.
                 *   - Return as response the new generated SessionID for this user.
                 */
                user.IPAddress = this.Request.UserHostAddress.ToString().Split(':')[0];
                user.TimeCreated = Time.GetTime();

                UsersManager.RemoveUserByUserName(userName);
                UsersManager.AddUser(user);

                return Response.AsJson(new MainResponse<string>(user.SessionID));
            }); 

            //ADMIN
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

                LocalUser member = null;

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
                    user.TimeRegisteredLocal = DateTime.Now;
                    Log.InsertNewLog(user.UserID, "user_to_owner");
                    return Response.AsJson(new MainResponse<AdminConfirmResponse>(new AdminConfirmResponse()
                    {
                        Password = Convert.ToBase64String(newPasswordBytes),
                        Id = user.UserID,
                        Name = user.Name
                    }));
                }
                else if (status == -3)
                    return Response.AsJson(new MainResponse<byte>(true, "member_is_already_an_admin"));
                else if (status == -4)
                    return Response.AsJson(new MainResponse<byte>(true, "member_id_not_found"));
                else
                    return Response.AsJson(new MainResponse<byte>(true, "database_error"));

            });

            //USER
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

                if (userId > 0)
                {
                    //SEND LOCKING DEVICE THE USERID AND WAIT UNTIL THE LOCKING DEVICE RETURNS THAT THE PROCESS IS GOING TO START
                    //IF NOT I SHOULD RETURN locker_device_not_found AND DELETE USER FROM DATABASE
                    /*List<string> responses = null;
                    try
                    {
                        responses = await Program.LockerDevicesManager.SendMessageToAllDevicesBlocking("create_user|" + userId + "|");
                    }
                    catch (Exception)
                    {

                    }

                    if (responses != null && responses.Count > 0)*/
                return Response.AsJson(new MainResponse<long>(userId));

                    /*LocalUser.DeleteUserFromDB(userId);
                    return Response.AsJson(new MainResponse<byte>(true, "locker_device_not_found"));*/
                }
                if(userId == -2)
                    return Response.AsJson(new MainResponse<byte>(true, "name_already_in_use"));
                return Response.AsJson(new MainResponse<byte>(true, "database_error"));
            });

            Post("/user/biometric", async (x, ct) =>
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
                if (!UsersManager.CheckLogIn(sessionId, this.Request.UserHostAddress, out user))
                    return Response.AsJson(new MainResponse<byte>(true, "expired_session_id"));

                LocalUser member = null;

                int status = LocalUser.GetUserFromDB(userId, out member);

                if (status == -1)
                    return Response.AsJson(new MainResponse<byte>(true, "database_error"));

                List<string> responses = null;
                try
                {
                    responses = await Program.LockerDevicesManager.SendMessageToAllDevicesBlocking("create_user|" + userId + "|");
                }
                catch (Exception)
                {

                }

                if (responses != null && responses.Count > 0)
                    return Response.AsJson(new MainResponse<string>("biometric_process_has_begun"));

                //LocalUser.DeleteUserFromDB(userId);
                return Response.AsJson(new MainResponse<byte>(true, "locker_device_not_found"));
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

                LocalUser member = null;

                int status = LocalUser.GetUserFromDB(userId, out member);

                if (status == -1)
                    return Response.AsJson(new MainResponse<byte>(true, "database_error"));


                List<string> responses = null;
                try
                {
                    responses = await Program.LockerDevicesManager.SendMessageToAllDevicesBlocking("get_photo_list|" + member.UserID + "|");
                }
                catch (Exception)
                {

                }

                if (responses != null && responses.Count > 0 && responses[0] != null)
                {
                    if (!responses[0].StartsWith("error"))
                    {
                        List<string> images = new List<string>();
                        string message = responses[0];
                        string value;
                        while ((value = getStringFirstValue(message, out message)) != String.Empty)
                        {
                            images.Add(LocalUser.URL_USER_IMAGE + userId + LocalUser.URL_USER_FACE_IMAGE + value);
                        }
                        return Response.AsJson(new MainResponse<List<string>>(images));
                    }
                    return Response.AsJson(new MainResponse<byte>(true, "no_images_found"));
                }

                return Response.AsJson(new MainResponse<byte>(true, "locker_device_not_found"));
            });

            Get("/user/{id}/face/{photo}", async (x, ct) =>
            {
                string sessionId = this.Request.Query["session_id"];
                string userIdString = x.id.ToString();
                string photoIdString = x.photo.ToString();
                uint userId = 0;
                int photoId = 0;

                if (sessionId == null || userIdString == null || !UInt32.TryParse(userIdString, out userId) || photoIdString == null || !Int32.TryParse(photoIdString, out photoId))
                {
                    return Response.AsJson(new MainResponse<byte>(true, "missing_request_parameters"));
                }

                LocalUser user;
                if (!UsersManager.CheckLogIn(sessionId, this.Request.UserHostAddress, out user))
                    return Response.AsJson(new MainResponse<byte>(true, "expired_session_id"));

                LocalUser member = null;

                int status = LocalUser.GetUserFromDB(userId, out member);

                if (status == -1)
                    return Response.AsJson(new MainResponse<byte>(true, "database_error"));

                List<byte[]> responses = null;
                try
                {
                    responses = await Program.LockerDevicesManager.GetImageUserRecognition(userId, photoId);
                }
                catch (Exception)
                {

                }

                if (responses != null && responses.Count > 0)
                    return Response.FromByteArray(responses[0], "image/png");

                return Response.AsJson(new MainResponse<byte>(true, "no_image_found"));
            });

            Get("/user/{id}/profile/image", async (x, ct) =>
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

                LocalUser member = null;

                int status = LocalUser.GetUserFromDB(userId, out member);

                if (status == -1)
                    return Response.AsJson(new MainResponse<byte>(true, "database_error"));

                string imageFormat = member.ProfilePhotoFormat;
                byte[] image = ProfilePhoto.GetProfilePhoto(userId, member.ProfilePhotoFormat == "image/jpeg" ? "jpg" : "png", out imageFormat);

                return Response.FromByteArray(image, imageFormat);
            });

            Post("/user/{id}/profile/image", async (x, ct) =>
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

                LocalUser member = null;

                int status = LocalUser.GetUserFromDB(userId, out member);

                if (status == -1)
                    return Response.AsJson(new MainResponse<byte>(true, "database_error"));

                var file = this.Request.Files.FirstOrDefault();

                if (file != null)
                {
                    if(file.ContentType != "image/jpeg" && file.ContentType != "image/png")
                    {
                        return Response.AsJson(new MainResponse<byte>(true, "no_file_uploaded"));
                    }

                    byte[] image = null;
                    try
                    {
                        using (MemoryStream ms = new MemoryStream())
                        {
                            file.Value.CopyTo(ms);
                            image = ms.ToArray();
                        }
                    }
                    catch(Exception)
                    {

                    }

                    if(image == null)
                        return Response.AsJson(new MainResponse<byte>(true, "database_error"));

                    if (!LocalUser.UpdateImageFormatUserFromDB(userId, file.ContentType == "image/jpeg" ? 1 : 2))
                        return Response.AsJson(new MainResponse<byte>(true, "database_error"));


                    ProfilePhoto.AddProfilePhoto(image, userId, file.ContentType == "image/jpeg" ? "jpg" : "png");
                    return Response.AsJson(new MainResponse<string>("image_uploaded"));
                }

                return Response.AsJson(new MainResponse<byte>(true, "no_file_uploaded"));
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

            //CAMERA
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
                //return Response.AsJson(new MainResponse<String>("tcp/h264://" + ipAddress + ":" + DotNetEnv.Env.GetInt("TCP_VIDEO_LISTENER_PORT").ToString()));
                
                List<byte[]> responses = null;
                try
                {
                    responses = await Program.LockerDevicesManager.GetLiveImageFromLockingDevices();
                }
                catch (Exception)
                {

                }

                if (responses != null && responses.Count > 0)
                    return Response.FromByteArray(responses[0], "image/jpeg");

                return Response.AsJson(new MainResponse<byte>(true, "no_live_image_available"));
            });

            //DOOR
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

                if (responses != null && responses.Count > 0)
                {
                    Log.InsertNewLog(user.UserID, "button_open_door");
                    return Response.AsJson(new MainResponse<List<string>>(responses));
                }

                return Response.AsJson(new MainResponse<byte>(true, "locker_device_not_found"));
            });

            Get("/reset", async (x, ct) =>
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
                    responses = await Program.LockerDevicesManager.SendMessageToAllDevicesBlocking("reset");
                }
                catch (Exception)
                {

                }

                if (responses != null && responses.Count > 0)
                {
                    //CLEAR DATABASE
                    LocalUser.WipeDatabase();
                    UsersManager.ClearUsers();
                    return Response.AsJson(new MainResponse<List<string>>(responses));
                }

                return Response.AsJson(new MainResponse<byte>(true, "locker_device_not_found"));
            });

            //LOGS & NOTIFICATIONS
            Get("/logs", async (x, ct) =>
            {
                string sessionId = this.Request.Query["session_id"];
                string beginDateString = this.Request.Query["begin_date"];
                string endDateString = this.Request.Query["end_date"];

                DateTime beginDate = DateTime.UtcNow;
                DateTime endDate = DateTime.UtcNow;

                if (sessionId == null || beginDateString == null || endDateString == null || !DateTime.TryParseExact(beginDateString, "dd/MM/yyyy-HH:mm:ss", null, DateTimeStyles.AssumeUniversal, out beginDate) || !DateTime.TryParseExact(endDateString, "dd/MM/yyyy-HH:mm:ss", null, DateTimeStyles.AssumeUniversal, out endDate))
                {
                    return Response.AsJson(new MainResponse<byte>(true, "missing_request_parameters"));
                }

                beginDate = beginDate.ToLocalTime();
                endDate = endDate.ToLocalTime();

                LocalUser user;
                if (!UsersManager.CheckLogIn(sessionId, this.Request.UserHostAddress, out user))
                    return Response.AsJson(new MainResponse<byte>(true, "expired_session_id"));

                List<Log> logs;
                int status = Log.GetLogs(beginDate, endDate, out logs);

                if (status < 0)
                    return Response.AsJson(new MainResponse<byte>(true, "database_error"));

                return Response.AsJson(new MainResponse<List<Log>>(logs));
            });

            Get("/notifications", async (x, ct) =>
            {
                string sessionId = this.Request.Query["session_id"];
                if (sessionId == null)
                {
                    return Response.AsJson(new MainResponse<byte>(true, "missing_request_parameters"));
                }

                LocalUser user;
                if (!UsersManager.CheckLogIn(sessionId, this.Request.UserHostAddress, out user))
                    return Response.AsJson(new MainResponse<byte>(true, "expired_session_id"));

                List<Log> logs;
                int status = Log.GetNotifications(user, out logs);

                if (status < 0)
                    return Response.AsJson(new MainResponse<byte>(true, "database_error"));

                return Response.AsJson(new MainResponse<List<Log>>(logs));
            });

            Put("/notification/{id}", async (x, ct) =>
            {
                string sessionId = this.Request.Query["session_id"];
                string notificationIdString = x.id.ToString();
                uint notificationId = 0;

                if (sessionId == null || notificationIdString == null || !UInt32.TryParse(notificationIdString, out notificationId))
                {
                    return Response.AsJson(new MainResponse<byte>(true, "missing_request_parameters"));
                }

                LocalUser user;
                if (!UsersManager.CheckLogIn(sessionId, this.Request.UserHostAddress, out user))
                    return Response.AsJson(new MainResponse<byte>(true, "expired_session_id"));

                if (Log.MarkNotificationAsRead(user, notificationId))
                    return Response.AsJson(new MainResponse<string>(""));
                else
                    return Response.AsJson(new MainResponse<byte>(true, "database_error"));

            });



#pragma warning restore CS1998
        }

        public static string getStringFirstValue(string message, out string substring)
        {
            int separatorIndex = message.IndexOf('|');
            if (separatorIndex == -1)
            {
                substring = String.Empty;
                return String.Empty;
            }

            substring = message.Substring(separatorIndex + 1);
            return message.Substring(0, separatorIndex);
        }
    }
}
