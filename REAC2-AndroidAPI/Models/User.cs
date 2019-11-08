using System;
using System.Collections.Generic;
using System.Text;

namespace REAC2_AndroidAPI.Models
{
    public class User
    {
        public string Name { get; set; }

        public string Posted { get; set; }

        public User(string name)
        {
            this.Name = name;
            this.Posted = "Nothing :-(";
        }
    }
}
