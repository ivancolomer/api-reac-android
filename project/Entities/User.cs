using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace REAC_AndroidAPI.Entities
{
    public class User
    {
        public bool IsOwner { get; set; }
        public string UserID { get; set; }
        public string Name { get; set; }
        public string SessionID { get; set; }
        public string IPAddress { get; set; }
        public long TimeCreated { get; set; }
    }
}
