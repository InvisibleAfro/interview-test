using HrmFileImport.Adapters;
using HrmFileImport.Options;
using HrmFileImport.Ports;
using HrmFileImport.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

internal class Program
{
    private static void Main(string[] args)
    {
        var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices((Action<HostBuilderContext, IServiceCollection>)((context, services) =>
    {
        var config = context.Configuration;

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

        services.AddTransient<FileImportService>();
        services.AddTransient<IHrmAuthClient, HrmAuthClient>();
        services.AddTransient<IEmployeeSource, SftpEmployeeSource>();
        services.AddHttpClient(HrmAuthClient.Client, (sp, client) =>
        {
            var opts = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<RestApiOptions>>().Value;
            client.BaseAddress = new Uri(opts.BaseUrl);
        });
    }))
    .Build();

        host.Run();
    }
}