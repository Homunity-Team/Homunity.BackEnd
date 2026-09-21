using System.Collections.Generic;

namespace Homunity_Web_Api.Properties
{
    public class PropertyCreatedResponse
    {
        public int PropertyId { get; set; }
        public List<string> Images { get; set; } = new();
        public string? Video { get; set; }
        public string Message { get; set; }
    }

}
