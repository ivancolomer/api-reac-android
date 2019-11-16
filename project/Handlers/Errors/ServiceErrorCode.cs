using System;
using System.Collections.Generic;
using System.Text;

namespace REAC_AndroidAPI.Handlers.Errors
{
    public enum ServiceErrorCode
    {
        GeneralError = 0,
        NotFound = 10,
        InternalServerError = 20,
        InvalidToken = 30,
        NotAcceptable = 40,
        MethodNotAllowed = 50,
    }
}
