using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductSynchronizer.Utils
{
    public class Error
    {
        public string ProductId { get; set; }
        public string Message { get; set; }
        public bool NeedToUpdateProductInDb { get; set; } = false;
    }
}
