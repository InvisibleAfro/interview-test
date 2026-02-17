using CSharpFunctionalExtensions;
using CsvHelper;
using CsvHelper.Configuration;
using HrmFileImport.Models.Csv;
using HrmFileImport.Ports;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Net;

namespace HrmFileImport.Services;

public class FileImportService
{
    private readonly ILogger<FileImportService> _logger;
    private readonly IHrmAuthClient _hrmAuthClient;
    private readonly IEmployeeSource _sftpEmployeeSource;
    private readonly IEmploymentRequestMapperMapper _employmentRequestMapper;
    private readonly IHrmApiClient _hrmApi;

    public FileImportService(ILogger<FileImportService> logger,
        IHrmAuthClient authClient, IEmployeeSource sftpEmployeeSource, IEmploymentRequestMapperMapper employmentRequestMapper,
        IHrmApiClient hrmApi)
    {
        _logger = logger;
        _hrmAuthClient = authClient;
        _sftpEmployeeSource = sftpEmployeeSource;
        _employmentRequestMapper = employmentRequestMapper;
        _hrmApi = hrmApi;
    }

    public async Task<Result> Import(CancellationToken ct = default)
    {
        _logger.LogInformation("Starting employee file import");
        try
        {
            //for one of job this is okay, but services with frequent calls should cache token.
            var token = await _hrmAuthClient.GetAccessTokenAsync(ct);
            _logger.LogInformation("Fetched access token from {AuthClient}", _hrmAuthClient.GetType().Name);

            await using var employeeCsvReader = await GetEmployeeCsv(ct);

            var totalRows = 0;
            var importedRows = 0;

            await foreach (var employee in employeeCsvReader.Csv.GetRecordsAsync<EmployeeCsvRow>(ct))
            {
                totalRows++;
                try
                {
                    _logger.LogInformation("Importing employee with EmploymentID {Id}", employee.EmploymentID);
                    var employmentRequest = _employmentRequestMapper.Map(employee);
                
                    await _hrmApi.PostEmployee(employmentRequest, token, ct);
                    importedRows++;
                }
                catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _logger.LogError(ex, "Unauthorized when calling HRM API. Aborting import.");
                    return Result.Failure("Unauthorized calling HRM API");
                }
                catch (HttpRequestException ex)
                {
                    _logger.LogError(ex, "HTTP error importing employee with EmploymentID {Id}", employee.EmploymentID);
                }catch(KeyNotFoundException ex)
                {
                    _logger.LogError(ex, "Mapping error for employee with EmploymentID {Id}.", employee.EmploymentID);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error importing employee with EmploymentID {Id}", employee.EmploymentID);
                }
            }

            if (totalRows != importedRows)
                _logger.LogWarning("Not all rows could be imported. Expected {TotalRows}, imported {ImportedRows}", totalRows, importedRows);
            else
                _logger.LogInformation("Import finished without errors. Total rows imported {TotalRows}", totalRows);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Unexpteced error. Failed to import employee records");
            return Result.Failure(ex.Message);
        }

        return Result.Success();
    }

    private async Task<EmployeeCsvScope> GetEmployeeCsv(CancellationToken ct)
    {
        var employees = await _sftpEmployeeSource.DownloadEmployeeSourceAsync(ct);
        var employeeReader = new StreamReader(employees);
        var csvReader = new CsvReader(employeeReader, CreateCsvReaderConfig());
        return new EmployeeCsvScope(employees, employeeReader, csvReader);
    }

    private CsvConfiguration CreateCsvReaderConfig()
    {
        return new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            Delimiter = ";",
            HasHeaderRecord = true,
            TrimOptions = TrimOptions.Trim,
            PrepareHeaderForMatch = args => args.Header.Trim(),
        };
    }

    private sealed class EmployeeCsvScope : IAsyncDisposable
    {
        public CsvReader Csv { get; }

        private readonly Stream _stream;
        private readonly StreamReader _reader;

        public EmployeeCsvScope(Stream stream, StreamReader reader, CsvReader csv)
        {
            _stream = stream;
            _reader = reader;
            Csv = csv;
        }

        public ValueTask DisposeAsync()
        {
            Csv.Dispose();
            _reader.Dispose();
            _stream.Dispose();
            return ValueTask.CompletedTask;
        }
    }
}
