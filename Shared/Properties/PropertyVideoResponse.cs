using System;


namespace Homunity_Web_Api.Properties
{
    public class PropertyVideoResponse
    {
        public int VideoId { get; set; }
        public int PropertyId { get; set; }
        public string VideoUrl { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
