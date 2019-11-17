using Nancy;
using System;
using System.Collections.Generic;
using System.Text;

namespace REAC_AndroidAPI.Utils.Responses
{
    public static class Extensions
    {
        public static Response FromByteArray(this IResponseFormatter formatter, byte[] body, string contentType = null)
        {
            return new ByteArrayResponse(body, contentType);
        }
    }
}
