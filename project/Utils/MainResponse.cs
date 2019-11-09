using System;
using System.Collections.Generic;
using System.Text;

namespace REAC_AndroidAPI.Utils
{
    public class MainResponse<T>
    {
        public bool Error { get; set; }
        public string ErrorMessage { get; set; }
        public T Content  { get; set; }

        public MainResponse()
            : this(false, "", default(T))
        {

        }

        public MainResponse(T content) 
            : this(false, "", content)
        {
        }

        public MainResponse(bool error, string errorMessage)
            : this(error, errorMessage, default(T))
        {
        }

        public MainResponse(bool error, string errorMessage, T content)
        {
            Error = error;
            ErrorMessage = errorMessage;
            Content = content;
        }
    }
}
