using System;

namespace Homunity_Web_Api.Properties
{
    public class PropertyImageResponse
    {
        public int ImageId { get; set; }
        public int PropertyId { get; set; }
        public string ImageUrl { get; set; }
        public DateTime CreatedAt { get; set; }
    }

}
