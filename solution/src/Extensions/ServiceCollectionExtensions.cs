using HrmFileImport.Adapters;
using HrmFileImport.Mappers;
using HrmFileImport.Ports;
using HrmFileImport.Services;
using Microsoft.Extensions.DependencyInjection;

namespace HrmFileImport.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddImportPipeline(this IServiceCollection services)
        {
            services.AddTransient<IHrmAuthClient, HrmAuthClient>();
            services.AddTransient<IEmployeeSource, SftpEmployeeSource>();
            services.AddSingleton<ICountryCodeMapper, CountryCodeMapper>();
            services.AddSingleton<IDateFormatter, DateFormatter>();
            services.AddSingleton<IEmploymentRequestMapperMapper, EmploymentRequestMapper>();
            services.AddTransient<FileImportService>();

            return services;
        }
    }
}
