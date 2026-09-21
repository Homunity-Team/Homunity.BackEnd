using Homunity_Buisness_Logic;
using Homunity_Data_Access.Entities;
using Homunity_Data_Access.Repositories;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Homunity.Tests.Unit
{
    public class ChatServiceHistoryMappingTests
    {
        private static (ChatService service, Mock<IChatRepository> chatRepo, Mock<IGeminiClient> gemini) BuildService()
        {
            var chatRepo = new Mock<IChatRepository>();
            var propertyRepo = new Mock<IPropertyRepository>();
            propertyRepo.Setup(r => r.GetActiveForChatAsync(It.IsAny<int>())).ReturnsAsync(new List<Homunity_Data_Access.Repositories.Models.PropertyChatProjection>());
            var universityRepo = new Mock<IUniversityRepository>();
            universityRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<UniversityEntity>());
            var gemini = new Mock<IGeminiClient>();

            var service = new ChatService(chatRepo.Object, propertyRepo.Object, universityRepo.Object, gemini.Object, Mock.Of<ILogger<ChatService>>());
            return (service, chatRepo, gemini);
        }

        [Fact]
        public async Task GetHistoryAsync_MapsEntitiesToChatMessageResponse()
        {
            var (service, chatRepo, _) = BuildService();
            chatRepo.Setup(r => r.GetHistoryAsync(1, 20)).ReturnsAsync(new List<ChatMessageEntity>
            {
                new ChatMessageEntity { MessageId = 1, StudentId = 1, Role = "user", Content = "hi", CreatedAt = DateTime.Now }
            });

            var result = await service.GetHistoryAsync(1);

            Assert.Single(result);
            Assert.Equal("hi", result[0].Content);
        }

        [Fact]
        public async Task SendMessageAsync_GeminiSucceeds_ReturnsGeneratedReply()
        {
            var (service, chatRepo, gemini) = BuildService();
            chatRepo.Setup(r => r.GetHistoryAsync(1, 10)).ReturnsAsync(new List<ChatMessageEntity>());
            gemini.Setup(g => g.GenerateReplyAsync(It.IsAny<string>(), It.IsAny<List<GeminiHistoryMessage>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(GeminiReplyResult.Ok("رد تجريبي من الذكاء الاصطناعي"));

            var response = await service.SendMessageAsync(1, "مرحباً");

            Assert.Equal("رد تجريبي من الذكاء الاصطناعي", response.Reply);
        }

        [Fact]
        public async Task SendMessageAsync_GeminiFails_ReturnsFallbackArabicMessage_AndDoesNotThrow()
        {
            var (service, chatRepo, gemini) = BuildService();
            chatRepo.Setup(r => r.GetHistoryAsync(1, 10)).ReturnsAsync(new List<ChatMessageEntity>());
            gemini.Setup(g => g.GenerateReplyAsync(It.IsAny<string>(), It.IsAny<List<GeminiHistoryMessage>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(GeminiReplyResult.Fail("timeout"));

            var response = await service.SendMessageAsync(1, "مرحباً");

            // فشل مزوّد خارجي لا يجب أن يوقف الطلب أو يرمي استثناء — يجب أن يرجع رسالة بديلة آمنة.
            Assert.Equal("عذراً، حدث خطأ داخلي.", response.Reply);
        }

        [Fact]
        public async Task SendMessageAsync_GeminiReturnsEmptyResponse_ReturnsCannotUnderstandMessage()
        {
            var (service, chatRepo, gemini) = BuildService();
            chatRepo.Setup(r => r.GetHistoryAsync(1, 10)).ReturnsAsync(new List<ChatMessageEntity>());
            gemini.Setup(g => g.GenerateReplyAsync(It.IsAny<string>(), It.IsAny<List<GeminiHistoryMessage>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(GeminiReplyResult.Fail("empty_response"));

            var response = await service.SendMessageAsync(1, "مرحباً");

            Assert.Equal("لم أستطع فهم السؤال.", response.Reply);
        }

        [Fact]
        public async Task SendMessageAsync_GeminiProviderError_ReturnsConnectivityMessage()
        {
            var (service, chatRepo, gemini) = BuildService();
            chatRepo.Setup(r => r.GetHistoryAsync(1, 10)).ReturnsAsync(new List<ChatMessageEntity>());
            gemini.Setup(g => g.GenerateReplyAsync(It.IsAny<string>(), It.IsAny<List<GeminiHistoryMessage>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(GeminiReplyResult.Fail("provider_error"));

            var response = await service.SendMessageAsync(1, "مرحباً");

            Assert.Equal("عذراً، يوجد مشكلة في الاتصال حالياً. حاول مرة أخرى.", response.Reply);
        }
    }
}