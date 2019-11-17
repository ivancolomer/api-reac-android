using Nancy;
using REAC_AndroidAPI.Utils;
using REAC_AndroidAPI.Utils.Responses;
using System;
using System.Collections.Generic;
using System.Text;

namespace REAC_AndroidAPI.Handlers.Errors
{
    public static class HttpServiceErrorDefinition
    {
        public static HttpServiceError NotFoundError = new HttpServiceError
        {
            HttpStatusCode = HttpStatusCode.NotFound,
            ServiceErrorModel = new MainResponse<ServiceErrorCode>
            {
                Error = true,
                Content = ServiceErrorCode.NotFound,
                ErrorMessage = "The requested entity was not found."
            }
        };

        public static HttpServiceError GeneralError = new HttpServiceError
        {
            HttpStatusCode = HttpStatusCode.BadRequest,
            ServiceErrorModel = new MainResponse<ServiceErrorCode>
            {
                Error = true,
                Content = ServiceErrorCode.GeneralError,
                ErrorMessage = "An error occured during processing the request."
            }
        };

        public static HttpServiceError InternalServerError = new HttpServiceError
        {
            HttpStatusCode = HttpStatusCode.InternalServerError,
            ServiceErrorModel = new MainResponse<ServiceErrorCode>
            {
                Error = true,
                Content = ServiceErrorCode.InternalServerError,
                ErrorMessage = "There was an internal server error during processing the request."
            }
        };

        public static HttpServiceError InvalidTokenError = new HttpServiceError
        {
            HttpStatusCode = HttpStatusCode.BadRequest,
            ServiceErrorModel = new MainResponse<ServiceErrorCode>
            {
                Error = true,
                Content = ServiceErrorCode.InvalidToken,
                ErrorMessage = "Invalid API Token."
            }
        };

        public static HttpServiceError NotAcceptableError = new HttpServiceError
        {
            HttpStatusCode = HttpStatusCode.NotAcceptable,
            ServiceErrorModel = new MainResponse<ServiceErrorCode>
            {
                Error = true,
                Content = ServiceErrorCode.NotAcceptable,
                ErrorMessage = "Not Acceptable."
            }
        };

        public static HttpServiceError MethodNotAllowedError = new HttpServiceError
        {
            HttpStatusCode = HttpStatusCode.MethodNotAllowed,
            ServiceErrorModel = new MainResponse<ServiceErrorCode>
            {
                Error = true,
                Content = ServiceErrorCode.MethodNotAllowed,
                ErrorMessage = "Method Not Allowed."
            }
        };
    }
}
