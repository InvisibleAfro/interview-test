using System.Text.Json.Serialization;

namespace HrmFileImport.Models.HrmApi
{
    public sealed class EmploymentDto
    {
        [JsonPropertyName("number")]
        public string? Number { get; set; }

        /// <summary>
        /// ISO 8601 
        /// </summary>
        [JsonPropertyName("startDate")]
        public string? StartDate { get; set; }

        /// <summary>
        /// Omit if blank
        /// </summary>
        [JsonPropertyName("endDate")]
        public string? EndDate { get; set; }

        [JsonPropertyName("fteFactor")]
        public decimal? FteFactor { get; set; }

        [JsonPropertyName("jobId")]
        public int? JobId { get; set; }

        [JsonPropertyName("orgUnitId")]
        public int? OrgUnitId { get; set; }

        [JsonPropertyName("location")]
        public string? Location { get; set; }

        /// <summary>
        /// Omit if blank
        /// </summary>
        [JsonPropertyName("managerId")]
        public int? ManagerId { get; set; }
    }

}
