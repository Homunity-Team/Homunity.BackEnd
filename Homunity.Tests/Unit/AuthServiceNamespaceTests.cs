using Homunity_Buisness_Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Homunity.Tests.Unit
{
    // يتحقق بشكل صريح أن AuthService أصبحت معرَّفة داخل namespace Homunity_Buisness_Logic
    // (كانت معرَّفة في الـglobal namespace قبل تعديل Sprint 4)، عبر التحقق من الـFullName الفعلي للـType.
    public class AuthServiceNamespaceTests
    {
        [Fact]
        public void AuthService_IsDefinedInsideCorrectNamespace()
        {
            var type = typeof(AuthService);
            Assert.Equal("Homunity_Buisness_Logic.AuthService", type.FullName);
        }

        [Fact]
        public void AuthService_ImplementsIAuthService()
        {
            Assert.True(typeof(IAuthService).IsAssignableFrom(typeof(AuthService)));
        }
    }
}
