﻿using System;
using System.Collections.Generic;
using System.Text;

namespace REAC2_AndroidAPI.Handlers
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
