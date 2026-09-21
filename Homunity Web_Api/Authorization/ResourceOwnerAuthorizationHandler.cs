using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Homunity_Web_Api.Authorization
{
    // Handler مركزي لقاعدة ملكية المصدر (Resource-based Authorization).
    // Admin مسموح له دايمًا. غير ذلك، لازم يكون الـUserId بتاع المستخدم الحالي موجود
    // جوه قائمة الـUserIds المسموح لهم (allowedOwnerIds) اللي بيبعتها الـController كـResource.
    public class ResourceOwnerAuthorizationHandler
        : AuthorizationHandler<ResourceOwnerRequirement, IEnumerable<int>>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            ResourceOwnerRequirement requirement,
            IEnumerable<int> resource)
        {
            if (context.User.IsInRole("Admin"))
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }

            var currentUserIdClaim = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(currentUserIdClaim, out var currentUserId) &&
                resource != null &&
                resource.Contains(currentUserId))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
