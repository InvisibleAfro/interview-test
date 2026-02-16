using System.Text.Json.Serialization;

namespace HrmFileImport.Models.HrmApi
{
    public sealed class PersonalIdentificationDto
    {
        [JsonPropertyName("idNumber")]
        public string? IdNumber { get; set; }

        [JsonPropertyName("idType")]
        public string? IdType { get; set; }

        /// <summary>
        /// ISO alpha-3, e.g. NOR
        /// </summary>
        [JsonPropertyName("idCountry")]
        public string? IdCountry { get; set; }
    }
}
