using HrmFileImport.Models.HrmApi;
using HrmFileImport.Options;
using HrmFileImport.Ports;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Text.Json;

namespace HrmFileImport.Adapters
{
    public class HrmApiClient : IHrmApiClient
    {
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly IHttpClientFactory _httpClientFactory;

        public const string Client = "HrmApi";

        public HrmApiClient(IOptions<JsonSerializerOptions> jsonOptions, IHttpClientFactory httpClientFactory)
        {
            _jsonOptions = jsonOptions.Value;
            _httpClientFactory = httpClientFactory;
        }
        public async Task PostEmployee(CreateEmploymentRequestDto employmentRequestDto, string authToken, CancellationToken ct)
        {
            var client = _httpClientFactory.CreateClient(Client);
            
            using var msg = new HttpRequestMessage(HttpMethod.Post, "/personnel/employments")
            {
                Content = JsonContent.Create(employmentRequestDto, options: _jsonOptions)
            };

            msg.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);
            using var resp = await client.SendAsync(msg, ct);

            if (!resp.IsSuccessStatusCode)
            {
                var body = await resp.Content.ReadAsStringAsync(ct);
                throw new HttpRequestException(
                    $"HRM API returned {(int)resp.StatusCode} {resp.ReasonPhrase}. Body: {body}");
            }
        }
    }
}
