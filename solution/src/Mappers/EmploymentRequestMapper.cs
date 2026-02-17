using HrmFileImport.Models.Csv;
using HrmFileImport.Models.HrmApi;
using HrmFileImport.Ports;
using System.Globalization;

namespace HrmFileImport.Mappers
{
    public class EmploymentRequestMapper : IEmploymentRequestMapperMapper
    {
        private readonly ICountryCodeMapper _countryMapper;
        private readonly IDateFormatter _dateFormatter;

        public EmploymentRequestMapper(
            ICountryCodeMapper countryMapper,
            IDateFormatter dateFormatter)
        {
            _countryMapper = countryMapper;
            _dateFormatter = dateFormatter;
        }

        public CreateEmploymentRequestDto Map(EmployeeCsvRow row)
        {
            var country = TrimOrNull(row.Country);
            var countryAlpha3 =  _countryMapper.ToAlpha3(country);
            var dialCode = _countryMapper.ToDialCode(country);

            return new CreateEmploymentRequestDto
            {
                User = new UserDto
                {
                    FirstName = TrimOrNull(row.FirstName),
                    LastName = TrimOrNull(row.LastName),
                    DateOfBirth = _dateFormatter.ToIsoUtcMidnight(row.DateOfBirth),
                    Gender = TrimOrNull(row.Gender),

                    EmailWork = TrimOrNull(row.EmailWork),
                    Email = TrimOrNull(row.EmailWork),
                    EmailPrivate = TrimOrNull(row.EmailPrivate),

                    ActiveDirectoryLogin = TrimOrNull(row.ActiveDirectoryLogin),

                    PhoneNumberWork = new PhoneNumberDto
                    {
                        PhoneNumber = RemoveSpaces(TrimOrNull(row.MobilePhone)),
                        DialCode = dialCode,
                        CountryCode = countryAlpha3
                    },

                    AddressInfo = new List<AddressInfoDto>
                    {
                        new AddressInfoDto
                        {
                            PostAddress = new PostAddressDto
                            {
                                Address = TrimOrNull(row.StreetAddress),
                                ZipCode = TrimOrNull(row.ZipCode),
                                City = TrimOrNull(row.City),
                                Country = countryAlpha3
                            }
                        }
                    }
                },

                PersonalIdentification = new PersonalIdentificationDto
                {
                    IdNumber = TrimOrNull(row.IdNumber),
                    IdType = TrimOrNull(row.IdType),
                    IdCountry = _countryMapper.ToAlpha3(TrimOrNull(row.IdCountry))
                },

                Employment = new EmploymentDto
                {
                    Number = TrimOrNull(row.EmploymentID),
                    StartDate = _dateFormatter.ToIsoUtcMidnight(row.StartDate),
                    EndDate = _dateFormatter.ToIsoUtcMidnightOrNull(row.EndDate),
                    FteFactor = ParseDecimalOrNull(row.FTEFactor),
                    JobId = ParseIntOrNull(row.JobID),
                    OrgUnitId = ParseIntOrNull(row.OrgUnitId),
                    Location = TrimOrNull(row.Location),
                    ManagerId = ParseIntOrNullBlankOmit(row.ManagerID)
                },

                CompanyEmployee = new CompanyEmployeeDto
                {
                    EmployeeId = TrimOrNull(row.EmployeeID)
                }
            };
        }

        #region helpers
        private static string? TrimOrNull(string? s)
            => string.IsNullOrWhiteSpace(s) ? null : s.Trim();

        private static string? RemoveSpaces(string? s)
            => s is null ? null : s.Replace(" ", "", StringComparison.Ordinal);

        private decimal? ParseDecimalOrNull(string? s)
        {
            s = TrimOrNull(s);
            if (s is null) return null;

            var normalized = s.Replace(',', '.');

            if (!decimal.TryParse(normalized, NumberStyles.Number, CultureInfo.InvariantCulture, out var value))
                throw new FormatException($"Invalid decimal '{s}'.");

            return value;
        }

        private int? ParseIntOrNull(string? s)
        {
            s = TrimOrNull(s);
            if (s is null) return null;

            if (!int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value))
                throw new FormatException($"Invalid integer '{s}'.");

            return value;
        }

        private  int? ParseIntOrNullBlankOmit(string? s)
            => string.IsNullOrWhiteSpace(s) ? null : ParseIntOrNull(s);
        #endregion
    }
}
