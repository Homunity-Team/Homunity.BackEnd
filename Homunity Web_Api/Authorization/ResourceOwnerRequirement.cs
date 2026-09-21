using Microsoft.AspNetCore.Authorization;

namespace Homunity_Web_Api.Authorization
{
    // متطلب (Requirement) يمثل قاعدة: "المستخدم الحالي هو مالك المصدر، أو Admin".
    // الـResource المرتبط بالمتطلب ده هو IEnumerable<int> يمثل قائمة الـUserIds المسموح لهم
    // بالوصول للمصدر (في أغلب الحالات مالك واحد، وفي حالة الحجز مالكين: الطالب صاحب الحجز
    // ومالك العقار معًا).
    public class ResourceOwnerRequirement : IAuthorizationRequirement
    {
    }
}
