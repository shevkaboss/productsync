using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductSynchronizer.Utils
{
    public class InnerException : Exception
    {
        public InnerException(int productId, string message, bool needToUpdateProductInDB = false) : base(message)
        {
            Error = new Error
            {
                ProductId = productId.ToString(),
                Message = message,
                NeedToUpdateProductInDb = needToUpdateProductInDB
            };
        }

        public InnerException(string message, bool needToUpdateProductInDB = false) : base(message)
        {
            Error = new Error
            {
                ProductId = "undefined",
                Message = message,
                NeedToUpdateProductInDb = needToUpdateProductInDB
            };
        }

        public Error Error { get; }
    }
}
