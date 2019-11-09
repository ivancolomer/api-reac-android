using Nancy;
using Nancy.Bootstrapper;
using Nancy.ErrorHandling;
using Nancy.Responses.Negotiation;
using Nancy.ViewEngines;
using REAC_AndroidAPI.Utils.Output;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace REAC_AndroidAPI.Handlers
{
    public static class CustomErrorHandler
    {
        public static void Enable(IPipelines pipelines, IResponseNegotiator responseNegotiator)
        {
            if (pipelines == null)
            {
                throw new ArgumentNullException("pipelines");
            }

            if (responseNegotiator == null)
            {
                throw new ArgumentNullException("responseNegotiator");
            }

            pipelines.OnError += (context, exception) => HandleException(context, exception, responseNegotiator);
        }

        private static void LogException(NancyContext context, Exception exception)
        {
            Logger.WriteLine("An exception occured during processing a request. (Exception=" + exception.ToString() + ").", Logger.LOG_LEVEL.WARN);
        }

        private static Response HandleException(NancyContext context, Exception exception, IResponseNegotiator responseNegotiator)
        {
            LogException(context, exception);

            return CreateNegotiatedResponse(context, responseNegotiator, exception);
        }

        private static Response CreateNegotiatedResponse(NancyContext context, IResponseNegotiator responseNegotiator, Exception exception)
        {
            HttpServiceError httpServiceError = HttpServiceErrorUtilities.ExtractFromException(exception, HttpServiceErrorDefinition.GeneralError);

            Negotiator negotiator = new Negotiator(context)
                .WithStatusCode(httpServiceError.HttpStatusCode)
                .WithModel(httpServiceError.ServiceErrorModel);

            return responseNegotiator.NegotiateResponse(negotiator, context);
        }
    }
}
