using REAC_AndroidAPI.Handlers;
using System;
using System.Collections.Generic;
using System.Text;

namespace REAC_AndroidAPI.Exceptions
{
    public class InvalidTokenErrorException : Exception, IHasHttpServiceError
    {
        public InvalidTokenErrorException()
            : base() { }

        public InvalidTokenErrorException(string message)
            : base(message) { }

        public InvalidTokenErrorException(string message, Exception innerException)
            : base(message, innerException) { }

        public HttpServiceError HttpServiceError { get { return HttpServiceErrorDefinition.InvalidTokenError; } }
    }
}
