using Nancy;
using REAC_AndroidAPI.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace REAC_AndroidAPI.Handlers
{
    public class HttpServiceError
    {
        public MainResponse<ServiceErrorCode> ServiceErrorModel { get; set; }

        public HttpStatusCode HttpStatusCode { get; set; }
    }
}
