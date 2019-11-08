

using Nancy;
using Nancy.Extensions;
using REAC2_AndroidAPI.Exceptions;
using REAC2_AndroidAPI.Handlers;
using REAC2_AndroidAPI.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace REAC2_AndroidAPI
{
    public class TestModule : NancyModule
    {
        public TestModule()
        {
            //Get("/", args => "Hello from Nancy running on CoreCLR");

            Get("/testing", async (x, ct) =>
            {
                return View["staticview", this.Request.Url];
            });

            Get("/", async (x, ct) =>
            {
                throw new NotFoundErrorException("OOPS");
                //await Task.Delay(1000);
                //await Task.Delay(1000);
                return Response.AsJson("Hello from Nancy running on CoreCLR");
            });


            Get("/login", async (x, ct) =>
            {
                string from = this.Request.Query["from"];
                string to = this.Request.Query["to"];

                //this.OnError.AddItemToEndOfPipeline((ctx, exception) => { });
                //throw


                return Response.AsJson(new List<Index>() {
                    new Index(from),
                });
            });

            Post("/user", async (x, ct) =>
            {
                await Task.Delay(1000);
                var jsonString = this.Request.Body.AsString();
                return Response.AsJson(jsonString);
            });
        }
    }
}
