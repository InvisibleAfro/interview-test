using HrmFileImport.Adapters;
using HrmFileImport.Extensions;
using HrmFileImport.Models.HrmApi;
using HrmFileImport.Options;
using HrmFileImport.Ports;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit;

namespace HrmFileImport.Tests
{ 
    public class ApiResilenceTests
    {
        [Fact]
        public async Task ApiResilence_PostEmployee_Retries_On503_AndEventuallySucceeds()
        {
            // Arrange: 1st call 503, 2nd call 201
            var handler = new CountingHandler((attempt, _) =>
                attempt == 1
                    ? new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)
                    : new HttpResponseMessage(HttpStatusCode.Created));

            var services = new ServiceCollection();
            services.AddSingleton<IOptions<RestApiOptions>>(new OptionsWrapper<RestApiOptions>(
                new RestApiOptions
                {
                    BaseUrl = "http://localhost:8080",
                    ClientId = "test",
                    ClientSecret = "test"
                })
            );

            services.Configure<JsonSerializerOptions>(o =>
            {
                o.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            });

            services.AddApiClients();
            services.AddHttpClient(HrmApiClient.Client)
                .ConfigurePrimaryHttpMessageHandler(() => handler);
            var sp = services.BuildServiceProvider();
            var api = sp.GetRequiredService<IHrmApiClient>();

            // Act
            await api.PostEmployee(new CreateEmploymentRequestDto(), "token", CancellationToken.None);

            // Assert: should have retried once
            Assert.Equal(2, handler.CallCount);
        }

        [Fact]
        public async Task ApiResilence_PostEmployee_Throws_AfterMaxRetries_OnPersistent503()
        {
            var handler = new CountingHandler((_, __) =>
                new HttpResponseMessage(HttpStatusCode.ServiceUnavailable));

            var services = new ServiceCollection();
            services.AddSingleton<IOptions<RestApiOptions>>(new OptionsWrapper<RestApiOptions>(
                new RestApiOptions
                {
                    BaseUrl = "http://localhost:8080",
                    ClientId = "test",
                    ClientSecret = "test"
                })
             );

            services.Configure<JsonSerializerOptions>(_ => { });
            services.AddApiClients();
            services.AddHttpClient(HrmApiClient.Client)
                .ConfigurePrimaryHttpMessageHandler(() => handler);

            var sp = services.BuildServiceProvider();
            var api = sp.GetRequiredService<IHrmApiClient>();

            await Assert.ThrowsAsync<HttpRequestException>(() =>
                api.PostEmployee(new CreateEmploymentRequestDto(), "token", CancellationToken.None));

            // 1 initial + 3 retries
            Assert.Equal(4, handler.CallCount);
        }

        #region test helpers
        public sealed class CountingHandler : HttpMessageHandler
        {
            private readonly Func<int, HttpRequestMessage, HttpResponseMessage> _responseFactory;
            private int _count;

            public int CallCount => _count;

            public CountingHandler(Func<int, HttpRequestMessage, HttpResponseMessage> responseFactory)
                => _responseFactory = responseFactory;

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                var attempt = Interlocked.Increment(ref _count);
                return Task.FromResult(_responseFactory(attempt, request));
            }
        }
        #endregion
    }
}