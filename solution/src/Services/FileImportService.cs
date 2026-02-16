using CSharpFunctionalExtensions;
using HrmFileImport.Ports;
using Microsoft.Extensions.Logging;

namespace HrmFileImport.Services;

public class FileImportService
{
    private readonly ILogger<FileImportService> _logger;
    private readonly IHrmAuthClient _authClient;
    private readonly IEmployeeSource _sftpEmployeeSource;

    public FileImportService(ILogger<FileImportService> logger,
        IHrmAuthClient authClient, IEmployeeSource sftpEmployeeSource)
    {
        _logger = logger;
        _authClient = authClient;
        _sftpEmployeeSource = sftpEmployeeSource;
    }

    public async Task<Result> Import(CancellationToken ct = default)
    {
        _logger.LogInformation("Starting employee file import");
        try
        {
            var token = await _authClient.GetAccessTokenAsync(ct);
            _logger.LogInformation("Fetched access token from {AuthClient}", _authClient.GetType().Name);


            var employees = await _sftpEmployeeSource.DownloadEmployeeSourceAsync(ct);

        }catch(Exception ex)
        {
            _logger.LogError(ex, "Failed to import employee records");
            return Result.Failure(ex.Message);
        }

        //todo add logic to process employee records and push to api.

        _logger.LogInformation("Finished employee file import");
        return Result.Success();
    }
}
