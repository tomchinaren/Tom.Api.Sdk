using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tom.Api
{
    public class ApiException : Exception
    {
        public ApiException(string message)
            : base(message)
        {
        }
        public ApiException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
