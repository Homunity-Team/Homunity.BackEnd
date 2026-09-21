using System;

namespace Homunity_Shared_DTOs.Properties
{
    // DTO للـ Read فقط - مش بيتبعت في الـ Request.
    // (Sprint 3 / Question 3): كانت هذه النسخة معلَّنة بدون namespace (global namespace) في الأصل.
    // أُضيف لها namespace صريح الآن ضمن تنظيف تكرار PropertyResponseDTO المُقرَّر في هذا الـSprint،
    // بدون أي تغيير في الخصائص أو السلوك.
    public class PropertyResponseDTO
    {
        public int PropertyID { get; set; }
        public int OwnerID { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int Rooms { get; set; }
        public string? PropertyType { get; set; }
        public int PropertyStatusID { get; set; }
        public string? RejectReason { get; set; }
        public DateTime CreatedAt { get; set; }
        public object? Location { get; set; }
        public object? Images { get; set; }
        public object? Video { get; set; }
        public object? Services { get; set; }
        public string? FullAddress { get; set; }
    }
}