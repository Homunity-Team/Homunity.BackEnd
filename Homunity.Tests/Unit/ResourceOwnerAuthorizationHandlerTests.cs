using Homunity_Web_Api.Authorization;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Homunity.Tests.Unit
{
    public class ResourceOwnerAuthorizationHandlerTests
    {
        private static AuthorizationHandlerContext BuildContext(IEnumerable<int> resource, int? userId, string? role = null)
        {
            var claims = new List<Claim>();
            if (userId.HasValue) claims.Add(new Claim(ClaimTypes.NameIdentifier, userId.Value.ToString()));
            if (!string.IsNullOrEmpty(role)) claims.Add(new Claim(ClaimTypes.Role, role));

            var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
            return new AuthorizationHandlerContext(new[] { new ResourceOwnerRequirement() }, principal, resource);
        }

        [Fact]
        public async Task Owner_IsAuthorized()
        {
            var context = BuildContext(resource: new[] { 5 }, userId: 5, role: "Student");
            await new ResourceOwnerAuthorizationHandler().HandleAsync(context);
            Assert.True(context.HasSucceeded);
        }

        [Fact]
        public async Task NonOwner_IsDenied()
        {
            var context = BuildContext(resource: new[] { 5 }, userId: 9, role: "Student");
            await new ResourceOwnerAuthorizationHandler().HandleAsync(context);
            Assert.False(context.HasSucceeded);
        }

        [Fact]
        public async Task Admin_IsAlwaysAuthorized_EvenIfNotInResourceList()
        {
            var context = BuildContext(resource: new[] { 5 }, userId: 999, role: "Admin");
            await new ResourceOwnerAuthorizationHandler().HandleAsync(context);
            Assert.True(context.HasSucceeded);
        }

        [Fact]
        public async Task MultipleAllowedOwners_SecondOwnerIsAuthorized()
        {
            // يحاكي حالة الحجز: الطالب صاحب الحجز (5) أو مالك العقار (7)
            var context = BuildContext(resource: new[] { 5, 7 }, userId: 7, role: "Owner");
            await new ResourceOwnerAuthorizationHandler().HandleAsync(context);
            Assert.True(context.HasSucceeded);
        }

        [Fact]
        public async Task UserNotInAnyAllowedOwnerList_IsDenied()
        {
            var context = BuildContext(resource: new[] { 5, 7 }, userId: 99, role: "Student");
            await new ResourceOwnerAuthorizationHandler().HandleAsync(context);
            Assert.False(context.HasSucceeded);
        }

        [Fact]
        public async Task MissingUserIdClaim_IsDenied()
        {
            var context = BuildContext(resource: new[] { 5 }, userId: null, role: "Student");
            await new ResourceOwnerAuthorizationHandler().HandleAsync(context);
            Assert.False(context.HasSucceeded);
        }
    }
}
