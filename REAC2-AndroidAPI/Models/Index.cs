using System;
using System.Collections.Generic;
using System.Text;

namespace REAC2_AndroidAPI.Models
{
    public class Index
    {
        public string Name { get; set; }

        public string Posted { get; set; }

        public Index(string name)
        {
            this.Name = name;
            this.Posted = "Nothing :-(";
        }
    }
}
