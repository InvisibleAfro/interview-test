using HrmFileImport.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace HrmFileImport;

public class ImportEmployeesTrigger
{
    private readonly FileImportService _importService;
    private readonly ILogger<ImportEmployeesTrigger> _logger;

    public ImportEmployeesTrigger(FileImportService importService, ILogger<ImportEmployeesTrigger> logger)
    {
        _importService = importService;
        _logger = logger;
    }

    [Function("ImportEmployees")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "import")] HttpRequest req,
        CancellationToken ct)
    {
        _logger.LogInformation("Employee import triggered");

        var result = await _importService.Import(ct);

        if (result.IsFailure)
        {
            _logger.LogError("Import failed: {Error}", result.Error);
            return new ObjectResult(new { error = result.Error }) { StatusCode = 500 };
        }

        return new OkObjectResult(new { message = "Import completed" });
    }
}
