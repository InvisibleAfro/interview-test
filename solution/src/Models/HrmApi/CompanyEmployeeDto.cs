using System.Text.Json.Serialization;

namespace HrmFileImport.Models.HrmApi
{
    public sealed class CompanyEmployeeDto
    {
        [JsonPropertyName("employeeId")]
        public string? EmployeeId { get; set; }
    }
}
