using System.Globalization;
using CSharpFunctionalExtensions;
using CsvHelper;
using CsvHelper.Configuration;
using HrmFileImport.Extensions;
using HrmFileImport.Mappers;
using HrmFileImport.Models;
using HrmFileImport.Ports;
using Microsoft.Extensions.Logging;

namespace HrmFileImport.Services;

public class FileImportService
{
    private readonly ILogger<FileImportService> _logger;

    public FileImportService(ILogger<FileImportService> logger)
    {
    }

    public async Task<Result> Import()
    {
        // In the beggining there was darkness.
    }
}
