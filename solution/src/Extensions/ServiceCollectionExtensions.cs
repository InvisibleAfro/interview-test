using HrmFileImport.Adapters;
using HrmFileImport.Mappers;
using HrmFileImport.Options;
using HrmFileImport.Ports;
using HrmFileImport.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Options;
using Polly;

namespace HrmFileImport.Extensions
{
    public static class ServiceCollectionExtensions
    {

        public static IServiceCollection AddImportPipeline(this IServiceCollection services)
        {
            services.AddApiClients();
            services.AddImportServices();
            return services;
        }

        public static IServiceCollection AddApiClients(this IServiceCollection services)
        {
            services.AddTransient<IHrmAuthClient, HrmAuthClient>();
            services.AddTransient<IHrmApiClient, HrmApiClient>();

            services.AddHttpClient(HrmAuthClient.Client, (sp, client) =>
            {
                var opts = sp.GetRequiredService<IOptions<RestApiOptions>>().Value;
                client.BaseAddress = new Uri(opts.BaseUrl);
            });

            services.AddHttpClient(HrmApiClient.Client, (sp, client) =>
            {
                var opts = sp.GetRequiredService<IOptions<RestApiOptions>>().Value;
                client.BaseAddress = new Uri(opts.BaseUrl);
            })
            .AddResilienceHandler("hrm-api-pipeline", builder =>
            {
                builder.AddRetry(new HttpRetryStrategyOptions
                {
                    MaxRetryAttempts = 3,
                    BackoffType = DelayBackoffType.Exponential,
                    UseJitter = true,
                    Delay = TimeSpan.FromMilliseconds(200),

                    ShouldHandle = args =>
                    {
                        return ValueTask.FromResult(
                            args.Outcome.Result?.StatusCode is >= System.Net.HttpStatusCode.InternalServerError
                            || args.Outcome.Exception is HttpRequestException);
                    }
                })
                .AddTimeout(TimeSpan.FromSeconds(10));
                builder.AddCircuitBreaker(new HttpCircuitBreakerStrategyOptions
                {
                    FailureRatio = 0.5,
                    SamplingDuration = TimeSpan.FromSeconds(30),
                    MinimumThroughput = 5,
                    BreakDuration = TimeSpan.FromSeconds(30)
                });
            });

            return services;
        }

        private static IServiceCollection AddImportServices(this IServiceCollection services)
        {
            services.AddTransient<IEmployeeSource, SftpEmployeeSource>();
            services.AddSingleton<ICountryCodeMapper, CountryCodeMapper>();
            services.AddSingleton<IDateFormatter, DateFormatter>();
            services.AddSingleton<IEmploymentRequestMapperMapper, EmploymentRequestMapper>();
            services.AddTransient<FileImportService>();

            return services;
        }
    }
}
