using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Homunity_Shared_DTOs
{
    public class MockProcessRequest
    {
        public string MockOrderId { get; set; }
        public string CardNumber { get; set; }  // test card
        public string CardExpiry { get; set; }
        public string CardCvv { get; set; }
    }
}
