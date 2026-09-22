using System;

namespace Homunity_Shared_DTOs.Properties
{
    public class PropertyImageResponse
    {
        public int ImageId { get; set; }
        public int PropertyId { get; set; }
        public string ImageUrl { get; set; }
        public DateTime CreatedAt { get; set; }
    }

}
