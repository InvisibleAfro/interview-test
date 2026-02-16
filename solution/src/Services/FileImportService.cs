using CSharpFunctionalExtensions;
using CsvHelper.Configuration;
using HrmFileImport.Models.Csv;
using HrmFileImport.Ports;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace HrmFileImport.Services;

public class FileImportService
{
    private readonly ILogger<FileImportService> _logger;
    private readonly IHrmAuthClient _authClient;
    private readonly IEmployeeSource _sftpEmployeeSource;
    private readonly IEmploymentRequestMapperMapper _employmentRequestMapper;

    public FileImportService(ILogger<FileImportService> logger,
        IHrmAuthClient authClient, IEmployeeSource sftpEmployeeSource, IEmploymentRequestMapperMapper employmentRequestMapper)
    {
        _logger = logger;
        _authClient = authClient;
        _sftpEmployeeSource = sftpEmployeeSource;
        _employmentRequestMapper = employmentRequestMapper;
    }

    public async Task<Result> Import(CancellationToken ct = default)
    {
        _logger.LogInformation("Starting employee file import");
        try
        {
            var token = await _authClient.GetAccessTokenAsync(ct);
            _logger.LogInformation("Fetched access token from {AuthClient}", _authClient.GetType().Name);

            var employees = await _sftpEmployeeSource.DownloadEmployeeSourceAsync(ct);
            using var employeeReader = new StreamReader(employees);
            using var employeeCsvReader = new CsvHelper.CsvReader(employeeReader, CreateCsvReaderConfig());
            await foreach (var employee in employeeCsvReader.GetRecordsAsync<EmployeeCsvRow>(ct))
            {
                _logger.LogInformation("Importing employee with EmploymentID {Id}", employee.EmploymentID);
                var employmentRequest = _employmentRequestMapper.Map(employee);
            }
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Failed to import employee records");
            return Result.Failure(ex.Message);
        }

        _logger.LogInformation("Finished employee file import");
        return Result.Success();
    }

    private CsvConfiguration CreateCsvReaderConfig()
    {
        return new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            Delimiter = ";",
            HasHeaderRecord = true,

            // optional but nice in real-world CSVs:
            TrimOptions = TrimOptions.Trim,
            PrepareHeaderForMatch = args => args.Header.Trim(),
        };
    }
}
