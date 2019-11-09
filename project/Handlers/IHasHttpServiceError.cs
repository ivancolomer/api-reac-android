using System;
using System.Collections.Generic;
using System.Text;

namespace REAC_AndroidAPI.Handlers
{
    public interface IHasHttpServiceError
    {
        HttpServiceError HttpServiceError { get; }
    }
}
