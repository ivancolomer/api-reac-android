using System;
using System.Collections.Generic;
using System.Text;

namespace REAC2_AndroidAPI.Handlers
{
    public interface IHasHttpServiceError
    {
        HttpServiceError HttpServiceError { get; }
    }
}
