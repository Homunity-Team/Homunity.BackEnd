using System.Net;
using System.Net.Http;
using System.Threading;
using Homunity_Buisness_Logic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Xunit;

namespace Homunity.Tests.Unit
{
    public class GeminiClientTests
    {
        private static IConfiguration BuildConfig()
        {
            var config = new Mock<IConfiguration>();
            config.Setup(c => c["Gemini:ApiKey"]).Returns("test-key");
            config.Setup(c => c["Gemini:ApiUrl"]).Returns("https://gemini.example.com/v1/generate");
            return config.Object;
        }

        private static HttpClient BuildHttpClient(HttpResponseMessage response)
        {
            var handler = new Mock<HttpMessageHandler>();
            handler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(response);
            return new HttpClient(handler.Object);
        }

        private static HttpClient BuildHttpClientThatThrows(Exception ex)
        {
            var handler = new Mock<HttpMessageHandler>();
            handler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(ex);
            return new HttpClient(handler.Object);
        }

        [Fact]
        public async Task GenerateReplyAsync_SuccessResponse_ReturnsReplyText()
        {
            var json = "{\"candidates\":[{\"content\":{\"parts\":[{\"text\":\"مرحباً بك\"}]}}]}";
            var httpClient = BuildHttpClient(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(json) });

            var client = new GeminiClient(httpClient, BuildConfig(), Mock.Of<ILogger<GeminiClient>>());
            var result = await client.GenerateReplyAsync("system", new List<GeminiHistoryMessage>());

            Assert.True(result.Success);
            Assert.Equal("مرحباً بك", result.ReplyText);
        }

        [Fact]
        public async Task GenerateReplyAsync_NonSuccessStatusCode_ReturnsProviderErrorFailure()
        {
            var httpClient = BuildHttpClient(new HttpResponseMessage(HttpStatusCode.InternalServerError) { Content = new StringContent("{}") });

            var client = new GeminiClient(httpClient, BuildConfig(), Mock.Of<ILogger<GeminiClient>>());
            var result = await client.GenerateReplyAsync("system", new List<GeminiHistoryMessage>());

            Assert.False(result.Success);
            Assert.Equal("provider_error", result.FailureReason);
        }

        [Fact]
        public async Task GenerateReplyAsync_NetworkFailure_ReturnsNetworkErrorFailure()
        {
            var httpClient = BuildHttpClientThatThrows(new HttpRequestException("connection refused"));

            var client = new GeminiClient(httpClient, BuildConfig(), Mock.Of<ILogger<GeminiClient>>());
            var result = await client.GenerateReplyAsync("system", new List<GeminiHistoryMessage>());

            Assert.False(result.Success);
            Assert.Equal("network_error", result.FailureReason);
        }

        [Fact]
        public async Task GenerateReplyAsync_MalformedJson_ReturnsInvalidResponseFormatFailure()
        {
            var httpClient = BuildHttpClient(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("not-json") });

            var client = new GeminiClient(httpClient, BuildConfig(), Mock.Of<ILogger<GeminiClient>>());
            var result = await client.GenerateReplyAsync("system", new List<GeminiHistoryMessage>());

            Assert.False(result.Success);
            Assert.Equal("invalid_response_format", result.FailureReason);
        }

        [Fact]
        public async Task GenerateReplyAsync_EmptyReplyText_ReturnsEmptyResponseFailure()
        {
            var json = "{\"candidates\":[{\"content\":{\"parts\":[{\"text\":\"\"}]}}]}";
            var httpClient = BuildHttpClient(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(json) });

            var client = new GeminiClient(httpClient, BuildConfig(), Mock.Of<ILogger<GeminiClient>>());
            var result = await client.GenerateReplyAsync("system", new List<GeminiHistoryMessage>());

            Assert.False(result.Success);
            Assert.Equal("empty_response", result.FailureReason);
        }
    }
}