using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace REAC_AndroidAPI.Entities
{
    public class Log
    {
        public uint LogID { get; set; }

        public uint UserID { get; set; }
        public string Name { get; set; }
        public string ProfilePhoto { get; set; }

        public string Date { get; set; } //TimeZone.CurrentTimeZone.ToUniversalTime()
        public string Info { get; set; }

    }
}
