using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Homunity_Shared_DTOs
{
    public class SuccessMessageResponse
    {
        public bool Success { get; set; } = true;
        public string Message { get; set; }
    }
}
