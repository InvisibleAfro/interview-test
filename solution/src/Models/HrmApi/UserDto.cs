using System.Text.Json.Serialization;

namespace HrmFileImport.Models.HrmApi
{
    public sealed class UserDto
    {
        [JsonPropertyName("firstName")]
        public string? FirstName { get; set; }

        [JsonPropertyName("lastName")]
        public string? LastName { get; set; }

        /// <summary>
        /// ISO 8601
        /// </summary>
        [JsonPropertyName("dateOfBirth")]
        public string? DateOfBirth { get; set; }

        [JsonPropertyName("gender")]
        public string? Gender { get; set; }

        /// <summary>
        /// Identitcal to EmailWork
        /// </summary>
        [JsonPropertyName("email")]
        public string? Email { get; set; }

        [JsonPropertyName("emailWork")]
        public string? EmailWork { get; set; }

        [JsonPropertyName("emailPrivate")]
        public string? EmailPrivate { get; set; }

        [JsonPropertyName("phoneNumberWork")]
        public PhoneNumberDto PhoneNumberWork { get; set; } = new();

        [JsonPropertyName("activeDirectoryLogin")]
        public string? ActiveDirectoryLogin { get; set; }

        [JsonPropertyName("addressInfo")]
        public List<AddressInfoDto> AddressInfo { get; set; } = new();
    }
}
