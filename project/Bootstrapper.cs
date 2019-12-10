using System;
using Nancy;
using Nancy.Responses.Negotiation;
using Nancy.TinyIoc;
using Nancy.Bootstrapper;

using REAC_AndroidAPI.Handlers.Errors;
using Nancy.Diagnostics;
using REAC_AndroidAPI.Utils.Output;
using Nancy.Extensions;
using System.IO;

namespace REAC_AndroidAPI
{
    public class Bootstrapper : DefaultNancyBootstrapper
    {
        protected override Func<ITypeCatalog, NancyInternalConfiguration> InternalConfiguration
        {
            get
            {
                return NancyInternalConfiguration.WithOverrides(config => {
                    config.StatusCodeHandlers = new[] { typeof(StatusCodeHandler404), typeof(StatusCodeHandler500), typeof(StatusCodeHandler406) };
                    config.ResponseProcessors = new[] { typeof(JsonProcessor) };
                });
            }
        }

        protected override void RequestStartup(TinyIoCContainer container, IPipelines pipelines, NancyContext context)
        {
            CustomErrorHandler.Enable(pipelines, container.Resolve<IResponseNegotiator>());
        }

        public override void Configure(Nancy.Configuration.INancyEnvironment environment)
        {
            //environment.Diagnostics(enabled: true, password: "password");
            environment.Tracing(enabled: true, displayErrorTraces: true);
        }

        protected override void ApplicationStartup(TinyIoCContainer container, IPipelines pipelines)
        {
            base.ApplicationStartup(container, pipelines);

            pipelines.BeforeRequest.AddItemToEndOfPipeline((ctx) => {
                Logger.WriteLineWithHeader(ctx.Request.Url.ToString(), "BeforeRequest - " + ctx.Request.UserHostAddress.ToString().Split(':')[0], Logger.LOG_LEVEL.DEBUG);
                //ctx.Request.Body.Seek(0, SeekOrigin.Begin);
                return null;
            });

            pipelines.AfterRequest.AddItemToEndOfPipeline((ctx) => {
                try
                {
                    var stream = new MemoryStream();
                    ctx.Response.Contents.Invoke(stream);

                    stream.Position = 0;
                    using (var reader = new StreamReader(stream))
                    {
                        Logger.WriteLineWithHeader(reader.ReadToEnd(), "AfterRequest - " + ctx.Request.UserHostAddress.ToString().Split(':')[0], Logger.LOG_LEVEL.DEBUG);
                    }
                }
                catch (Exception) { }
            });
        }
    }
}
