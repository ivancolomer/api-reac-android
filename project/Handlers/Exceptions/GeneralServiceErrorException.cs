using REAC_AndroidAPI.Handlers.Errors;
using System;
using System.Collections.Generic;
using System.Text;

namespace REAC_AndroidAPI.Handlers.Exceptions
{
    public class GeneralServiceErrorException : Exception, IHasHttpServiceError
    {
        public GeneralServiceErrorException()
            : base() { }

        public GeneralServiceErrorException(string message)
            : base(message) { }

        public GeneralServiceErrorException(string message, Exception innerException)
            : base(message, innerException) { }

        public HttpServiceError HttpServiceError { get { return HttpServiceErrorDefinition.GeneralError; } }
    }
}
