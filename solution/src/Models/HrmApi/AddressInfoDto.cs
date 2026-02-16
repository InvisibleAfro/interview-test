using System.Text.Json.Serialization;

namespace HrmFileImport.Models.HrmApi
{
    public sealed class AddressInfoDto
    {
        [JsonPropertyName("postAddress")]
        public PostAddressDto PostAddress { get; set; } = new();
    }
}
