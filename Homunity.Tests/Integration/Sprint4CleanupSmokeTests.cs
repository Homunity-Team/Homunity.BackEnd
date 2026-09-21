using Microsoft.AspNetCore.Mvc.Testing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace Homunity.Tests.Integration
{
    // اختبارات دخان للتأكد من أن التطبيق يبني ويعمل بنجاح بعد تنظيف Sprint 4
    // (حذف ملفات ميتة، نقل حزم NuGet بين المشاريع، توحيد namespace في AuthService).
    // لا يوجد منطق عمل جديد ليُختبَر في هذا الـSprint.
    public class Sprint4CleanupSmokeTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        public Sprint4CleanupSmokeTests(WebApplicationFactory<Program> factory) => _factory = factory;

        [Fact]
        public async Task Application_StartsSuccessfully_AndSwaggerIsReachable()
        {
            // يتحقق ضمنيًا أن Program.cs بني بنجاح (بما فيه AuthService بعد إضافة الـnamespace)
            // وأن كل التسجيلات في DI ما زالت سليمة بعد كل عمليات الحذف.
            var client = _factory.CreateClient();
            var response = await client.GetAsync("/swagger/v1/swagger.json");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Login_EndpointStillReachable_AfterAuthServiceNamespaceFix()
        {
            // يتأكد أن AuthController → IAuthService → AuthService (بعد تغليفها بالـnamespace)
            // لسه متصل بشكل صحيح عبر DI، ويرد بـ400 المتوقع لطلب ناقص، لا بخطأ تسجيل DI (500).
            var client = _factory.CreateClient();
            var response = await client.PostAsJsonAsync("/api/Auth/Login", new { phone = "", password = "" });
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task CreateFullProperty_EndpointStillReachable_AfterDtoDuplicateRemoval()
        {
            // يتأكد أن حذف Homunity_Shared_DTOs.Properties.CreateFullPropertyDTO الميتة
            // لم يكسر مسار PropertiesController.CreateFullProperty (يستخدم النسخة الحية في Business Logic).
            var client = _factory.CreateClient();
            using var content = new MultipartFormDataContent();
            var response = await client.PostAsync("/api/Properties/CreateFullProperty", content);
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task RegisterUser_EndpointStillReachable_AfterUserEntityRename()
        {
            // يتأكد أن إعادة تسمية UserEntitycs.cs → UserEntity.cs (وحذف الملف الفاضي القديم)
            // لم تكسر مسار التسجيل الذي يعتمد على هذا الكيان عبر EF Core.
            var client = _factory.CreateClient();
            var content = new StringContent("{}", System.Text.Encoding.UTF8, "application/json");
            var response = await client.PostAsync("/api/Users/Register", content);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}
