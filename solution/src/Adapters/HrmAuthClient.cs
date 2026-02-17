using HrmFileImport.Options;
using HrmFileImport.Ports;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace HrmFileImport.Adapters
{
    public class HrmAuthClient : IHrmAuthClient
    {
        public const string Client = "HrmAuth";
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly RestApiOptions _options;

        public HrmAuthClient(IHttpClientFactory httpClientFactory, IOptions<RestApiOptions> options)
        {
            _httpClientFactory = httpClientFactory;
            _options = options.Value;
        }

        public async Task<string> GetAccessTokenAsync(CancellationToken ct = default)
        {
            var client = _httpClientFactory.CreateClient(Client);

            using var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "client_credentials",
                ["client_id"] = _options.ClientId,
                ["client_secret"] = _options.ClientSecret
            });

            using var resp = await client.PostAsync("/token", content, ct);
            resp.EnsureSuccessStatusCode();

            using var stream = await resp.Content.ReadAsStreamAsync(ct);
            using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: ct);

            if (!doc.RootElement.TryGetProperty("access_token", out var token))
                throw new InvalidOperationException("Failed to fetch access token.");

            var value = token.GetString();
            return value ?? throw new InvalidOperationException("Failed to fetch access token.");
        }
    }
}
