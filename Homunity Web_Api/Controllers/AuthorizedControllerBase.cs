using Homunity_Web_Api.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Homunity_Web_Api.Controllers
{
    // كل الكنترولرز المحمية هترث من الكلاس ده بدل ControllerBase مباشرة
    public abstract class AuthorizedControllerBase : ControllerBase
    {
        protected int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        protected bool IsAdmin => User.IsInRole("Admin");

        // Sprint 2: نقطة مركزية موحّدة للتحقق من ملكية المصدر (Resource Ownership)،
        // بدلاً من تكرار "if (id != CurrentUserId && !IsAdmin) return Forbid();" يدويًا
        // داخل كل Action. بتقبل أكتر من مالك مسموح له (مثال: في الحجز، الطالب صاحب
        // الحجز أو مالك العقار مسموح لهم الاتنين). الـAdmin مسموح له دايمًا بشكل مركزي
        // جوه الـHandler، بدون أي تكرار هنا.
        protected async Task<bool> IsAuthorizedForResourceAsync(params int[] allowedOwnerIds)
        {
            var authorizationService = HttpContext.RequestServices.GetRequiredService<IAuthorizationService>();
            var result = await authorizationService.AuthorizeAsync(User, allowedOwnerIds, PolicyNames.ResourceOwner);
            return result.Succeeded;
        }
    }
}
