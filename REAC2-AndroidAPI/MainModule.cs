using Nancy;
using Nancy.Extensions;
using REAC_AndroidApi.Utils;
using REAC2_AndroidAPI.Exceptions;
using REAC2_AndroidAPI.Handlers;
using REAC2_AndroidAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace REAC2_AndroidAPI
{
    public class MainModule : NancyModule
    {
        public MainModule()
            : base("api")
        {

#pragma warning disable CS1998 // El método asincrónico carece de operadores "await" y se ejecutará de forma sincrónica

            Get("/", async (x, ct) =>
            {
                return Response.AsJson(new MainResponse<String>("Welcome to AndroidAPI"));
            });



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

                Guid guid = Guid.NewGuid();
                var guidBytes = guid.ToByteArray();

                long timeNow = Time.GetTime();

                //Send query to mysql
                if(false)
                {
                    return Response.AsJson(new MainResponse<byte>(true, "Error while processing your request(1)"));
                }

                return Response.AsJson(new MainResponse<String>(BitConverter.ToString(guidBytes)));
            });

            Post("/user", async (x, ct) =>
            {
                await Task.Delay(1000);
                var jsonString = this.Request.Body.AsString();
                return Response.AsJson(jsonString);
                /*return Response.AsJson(new List<User>() {
                    new User(userId),
                });*/
            });

#pragma warning restore CS1998 // El método asincrónico carece de operadores "await" y se ejecutará de forma sincrónica
        }
    }
}
