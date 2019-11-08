using Nancy;
using REAC2_AndroidAPI.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace REAC2_AndroidAPI.Handlers
{
    public class HttpServiceError
    {
        public ServiceErrorModel ServiceErrorModel { get; set; }

        public HttpStatusCode HttpStatusCode { get; set; }
    }
}
