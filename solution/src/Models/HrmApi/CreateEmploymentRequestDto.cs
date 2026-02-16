using System.Text.Json.Serialization;

namespace HrmFileImport.Models.HrmApi
{
    public sealed class CreateEmploymentRequestDto
    {
        [JsonPropertyName("user")]
        public UserDto User { get; set; } = new();

        [JsonPropertyName("personalIdentification")]
        public PersonalIdentificationDto PersonalIdentification { get; set; } = new();

        [JsonPropertyName("employment")]
        public EmploymentDto Employment { get; set; } = new();

        [JsonPropertyName("companyEmployee")]
        public CompanyEmployeeDto CompanyEmployee { get; set; } = new();
    }
}
