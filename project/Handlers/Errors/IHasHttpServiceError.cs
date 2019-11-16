using System;
using System.Collections.Generic;
using System.Text;

namespace REAC_AndroidAPI.Handlers.Errors
{
    public interface IHasHttpServiceError
    {
        HttpServiceError HttpServiceError { get; }
    }
}
