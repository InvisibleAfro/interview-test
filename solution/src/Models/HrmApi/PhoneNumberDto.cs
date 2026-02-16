using System.Text.Json.Serialization;

namespace HrmFileImport.Models.HrmApi
{
    public sealed class PhoneNumberDto
    {
        [JsonPropertyName("phoneNumber")]
        public string? PhoneNumber { get; set; }

        [JsonPropertyName("dialCode")]
        public string? DialCode { get; set; }

        /// <summary>
        /// ISO alpha-3
        /// </summary>
        [JsonPropertyName("countryCode")]
        public string? CountryCode { get; set; }
    }
}
