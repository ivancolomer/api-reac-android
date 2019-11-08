using REAC2_AndroidAPI.Handlers;
using System;
using System.Collections.Generic;
using System.Text;

namespace REAC2_AndroidAPI.Exceptions
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
