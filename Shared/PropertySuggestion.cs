using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Homunity_Shared_DTOs
{
    public class PropertySuggestion
    {
        public int PropertyID { get; set; }
        public string Title { get; set; }
        public decimal Price { get; set; }
        public string Address { get; set; }
        public string ImageUrl { get; set; }
        public string University { get; set; }
        public double? Distance { get; set; }
    }
}
