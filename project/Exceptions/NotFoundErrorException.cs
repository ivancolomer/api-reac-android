using REAC_AndroidAPI.Handlers;
using System;
using System.Collections.Generic;
using System.Text;

namespace REAC_AndroidAPI.Exceptions
{
    public class NotFoundErrorException : Exception, IHasHttpServiceError
    {
        public NotFoundErrorException()
            : base() { }

        public NotFoundErrorException(string message)
            : base(message) { }

        public NotFoundErrorException(string message, Exception innerException)
            : base(message, innerException) { }

        public HttpServiceError HttpServiceError { get { return HttpServiceErrorDefinition.NotFoundError; } }
    }
}
