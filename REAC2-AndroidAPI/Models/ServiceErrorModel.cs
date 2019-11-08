using REAC2_AndroidAPI.Handlers;
using System;
using System.Collections.Generic;
using System.Text;

namespace REAC2_AndroidAPI.Models
{
    public class ServiceErrorModel
    {
        public ServiceErrorCode Code { get; set; }

        public string Details { get; set; }
    }
}
