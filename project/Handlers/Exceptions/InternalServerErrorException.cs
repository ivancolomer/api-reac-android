
using REAC_AndroidAPI.Handlers.Errors;
using System;
using System.Collections.Generic;
using System.Text;

namespace REAC_AndroidAPI.Handlers.Exceptions
{
    public class InternalServerErrorException : Exception, IHasHttpServiceError
    {
        public InternalServerErrorException()
            : base() { }

        public InternalServerErrorException(string message)
            : base(message) { }

        public InternalServerErrorException(string message, Exception innerException)
            : base(message, innerException) { }

        public HttpServiceError HttpServiceError { get { return HttpServiceErrorDefinition.InternalServerError; } }
    }
}
