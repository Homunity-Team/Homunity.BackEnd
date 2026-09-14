using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Homunity_Web_Api.Controllers
{
    // كل الكنترولرز المحمية هترث من الكلاس ده بدل ControllerBase مباشرة
    public abstract class AuthorizedControllerBase : ControllerBase
    {
        protected int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        protected bool IsAdmin => User.IsInRole("Admin");
    }
}
