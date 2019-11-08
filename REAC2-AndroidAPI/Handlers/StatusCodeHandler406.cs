using Nancy;
using Nancy.ErrorHandling;
using Nancy.Responses.Negotiation;
using REAC2_AndroidAPI.Utils.Output;
using System;
using System.Collections.Generic;
using System.Text;

namespace REAC2_AndroidAPI.Handlers
{
    public class StatusCodeHandler406 : IStatusCodeHandler
    {
        private IResponseNegotiator responseNegotiator;

        public StatusCodeHandler406(IResponseNegotiator responseNegotiator)
        {
            this.responseNegotiator = responseNegotiator;
        }

        public bool HandlesStatusCode(HttpStatusCode statusCode, NancyContext context)
        {
            //return statusCode != HttpStatusCode.InternalServerError;
            //Logger.WriteLine(statusCode.ToString(), Logger.LOG_LEVEL.INFO);
            return statusCode == HttpStatusCode.NotAcceptable;
        }

        public void Handle(HttpStatusCode statusCode, NancyContext context)
        {
            context.NegotiationContext = new NegotiationContext();

            Negotiator negotiator = new Negotiator(context)
                .WithStatusCode(HttpServiceErrorDefinition.NotAcceptableError.HttpStatusCode)
                .WithModel(HttpServiceErrorDefinition.NotAcceptableError.ServiceErrorModel);

            context.Response = responseNegotiator.NegotiateResponse(negotiator, context);
        }
    }
}
