using System;
using Nancy;
using Nancy.Responses.Negotiation;
using Nancy.TinyIoc;
using Nancy.Bootstrapper;

using REAC_AndroidAPI.Handlers.Errors;
using Nancy.Diagnostics;
using REAC_AndroidAPI.Utils.Output;

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
    }
}
