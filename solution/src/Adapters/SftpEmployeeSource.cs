using HrmFileImport.Options;
using HrmFileImport.Ports;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Renci.SshNet;

namespace HrmFileImport.Adapters
{
    public class SftpEmployeeSource : IEmployeeSource
    {
        private readonly ILogger<SftpEmployeeSource> _logger;
        private readonly SftpOptions _options;
        public SftpEmployeeSource(IOptions<SftpOptions> options, ILogger<SftpEmployeeSource> logger)
        {
            _options = options.Value;
            _logger = logger;
        }

        public async Task<Stream> DownloadEmployeeSourceAsync(CancellationToken ct = default)
        {
            _logger.LogInformation("Connecting to SFTP server at {Host}:{Port} as {User}", _options.Host, _options.Port, _options.Username);

            // Renci.SshNet is synchronous; run blocking operations on a thread pool thread and honor cancellation
            return await Task.Run(() =>
            {
                using var client = new SftpClient(_options.Host, _options.Port, _options.Username, _options.Password);
                try
                {
                    client.Connect();

                    var remotePath = $"{_options.Directory}/employees.csv";

                    if (!client.Exists(remotePath))
                    {
                        throw new FileNotFoundException($"File not found at {remotePath} on SFTP server");
                    }

                    var ms = new MemoryStream();
                    client.DownloadFile(remotePath, ms);
                    ms.Position = 0;
                    return (Stream)ms;
                }
                finally
                {
                    if (client.IsConnected)
                        client.Disconnect();
                }
            }, ct);
        }
    }
}
