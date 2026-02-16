using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace HrmFileImport.Models.HrmApi
{
    public sealed class PostAddressDto
    {
        [JsonPropertyName("address")]
        public string? Address { get; set; }

        [JsonPropertyName("zipCode")]
        public string? ZipCode { get; set; }

        [JsonPropertyName("city")]
        public string? City { get; set; }

        /// <summary>
        /// ISO alpha-3
        /// </summary>
        [JsonPropertyName("country")]
        public string? Country { get; set; }
    }
}
