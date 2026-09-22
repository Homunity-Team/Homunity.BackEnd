using System;


namespace Homunity_Shared_DTOs.Properties
{
    public class PropertyVideoResponse
    {
        public int VideoId { get; set; }
        public int PropertyId { get; set; }
        public string VideoUrl { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
