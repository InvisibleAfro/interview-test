using HrmFileImport.Adapters;
using HrmFileImport.Extensions;
using HrmFileImport.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Http.Resilience;
using Polly;

internal class Program
{
    private static void Main(string[] args)
    {
        var host = new HostBuilder()
            .ConfigureFunctionsWebApplication()
            .ConfigureServices((context, services) =>
            {
                var config = context.Configuration;

                services.Configure<JsonSerializerOptions>(options =>
                {
                    options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                });

                services.AddOptions<RestApiOptions>()
                    .Bind(config.GetSection("HrmApi"))
                    .Validate(o =>
                        !string.IsNullOrWhiteSpace(o.BaseUrl) &&
                        !string.IsNullOrWhiteSpace(o.ClientId) &&
                        !string.IsNullOrWhiteSpace(o.ClientSecret),
                        "Missing HrmApi settings")
                    .ValidateOnStart();

                services.AddOptions<SftpOptions>()
                    .Bind(config.GetSection("SftpEmployeeSource"))
                    .Validate(o =>
                        !string.IsNullOrWhiteSpace(o.Host) &&
                        o.Port > 0 &&
                        !string.IsNullOrWhiteSpace(o.Username) &&
                        !string.IsNullOrWhiteSpace(o.Password) &&
                        !string.IsNullOrWhiteSpace(o.Directory),
                        "Missing Sftp settings")
                    .ValidateOnStart();

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
                services.AddImportPipeline();
            })
            .Build();

        host.Run();
    }
}