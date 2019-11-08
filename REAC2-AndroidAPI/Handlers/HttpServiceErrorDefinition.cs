using Nancy;
using REAC2_AndroidAPI.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace REAC2_AndroidAPI.Handlers
{
    public static class HttpServiceErrorDefinition
    {
        public static HttpServiceError NotFoundError = new HttpServiceError
        {
            HttpStatusCode = HttpStatusCode.NotFound,
            ServiceErrorModel = new ServiceErrorModel
            {
                Code = ServiceErrorCode.NotFound,
                Details = "The requested entity was not found."
            }
        };

        public static HttpServiceError GeneralError = new HttpServiceError
        {
            HttpStatusCode = HttpStatusCode.BadRequest,
            ServiceErrorModel = new ServiceErrorModel
            {
                Code = ServiceErrorCode.GeneralError,
                Details = "An error occured during processing the request."
            }
        };

        public static HttpServiceError InternalServerError = new HttpServiceError
        {
            HttpStatusCode = HttpStatusCode.InternalServerError,
            ServiceErrorModel = new ServiceErrorModel
            {
                Code = ServiceErrorCode.InternalServerError,
                Details = "There was an internal server error during processing the request."
            }
        };

        public static HttpServiceError InvalidTokenError = new HttpServiceError
        {
            HttpStatusCode = HttpStatusCode.BadRequest,
            ServiceErrorModel = new ServiceErrorModel
            {
                Code = ServiceErrorCode.InvalidToken,
                Details = "Invalid API Token."
            }
        };

        public static HttpServiceError NotAcceptableError = new HttpServiceError
        {
            HttpStatusCode = HttpStatusCode.NotAcceptable,
            ServiceErrorModel = new ServiceErrorModel
            {
                Code = ServiceErrorCode.NotAcceptable,
                Details = "Not Acceptable."
            }
        };

        public static HttpServiceError MethodNotAllowedError = new HttpServiceError
        {
            HttpStatusCode = HttpStatusCode.MethodNotAllowed,
            ServiceErrorModel = new ServiceErrorModel
            {
                Code = ServiceErrorCode.MethodNotAllowed,
                Details = "Method Not Allowed."
            }
        };
    }
}
