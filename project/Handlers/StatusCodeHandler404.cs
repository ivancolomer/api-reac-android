using Nancy;
using Nancy.ErrorHandling;
using Nancy.Responses.Negotiation;
using REAC_AndroidAPI.Utils.Output;
using System;
using System.Collections.Generic;
using System.Text;

namespace REAC_AndroidAPI.Handlers
{
    public class StatusCodeHandler404 : IStatusCodeHandler
    {
        private IResponseNegotiator responseNegotiator;

        public StatusCodeHandler404(IResponseNegotiator responseNegotiator)
        {
            this.responseNegotiator = responseNegotiator;
        }

        public bool HandlesStatusCode(HttpStatusCode statusCode, NancyContext context)
        {
            //return statusCode != HttpStatusCode.InternalServerError;
            //Logger.WriteLine(statusCode.ToString(), Logger.LOG_LEVEL.INFO);
            return statusCode == HttpStatusCode.NotFound;
        }

        public void Handle(HttpStatusCode statusCode, NancyContext context)
        {
            context.NegotiationContext = new NegotiationContext();

            Negotiator negotiator = new Negotiator(context)
                .WithStatusCode(HttpServiceErrorDefinition.NotFoundError.HttpStatusCode)
                .WithModel(HttpServiceErrorDefinition.NotFoundError.ServiceErrorModel);

            context.Response = responseNegotiator.NegotiateResponse(negotiator, context);
        }
    }
}
