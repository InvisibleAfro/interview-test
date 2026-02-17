using HrmFileImport.Extensions;
using HrmFileImport.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Text.Json;
using System.Text.Json.Serialization;

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

                services.AddImportPipeline();
            })
            .Build();

        host.Run();
    }
}