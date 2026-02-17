using HrmFileImport.Mappers;
using HrmFileImport.Models.Csv;
using HrmFileImport.Ports;
using Xunit;

namespace HrmFileImport.Tests;

public sealed class EmploymentRequestMapperTests
{
    [Fact]
    public void Map_MobilePhone_RemovesSpaces()
    {
        // Arrange
        ICountryCodeMapper country = new CountryCodeMapper();
        IDateFormatter dates = new DateFormatter();
        var requestMapper = new EmploymentRequestMapper(country, dates);
        var row = CreateValidRow();

        // Act
        var dto = requestMapper.Map(row);

        // Assert
        Assert.NotNull(dto.User);
        Assert.NotNull(dto.User.PhoneNumberWork);
        Assert.Equal("91234567", dto.User.PhoneNumberWork.PhoneNumber);
    }

    [Fact]
    public void Map_DateOfBirth_FormatsToIsoUtcMidnight()
    {
        // Arrange
        ICountryCodeMapper country = new CountryCodeMapper();
        IDateFormatter dates = new DateFormatter();
        var requestMapper = new EmploymentRequestMapper(country, dates);
        var row = CreateValidRow();

        // Act
        var dto = requestMapper.Map(row);

        // Assert
        Assert.NotNull(dto.User);
        Assert.Equal("1990-02-01T00:00:00+0000", dto.User.DateOfBirth);
    }

    private static EmployeeCsvRow CreateValidRow() =>
        new(
            FirstName: "Ola",
            LastName: "Nordmann",
            DateOfBirth: "01.02.1990",
            Gender: "M",
            EmailWork: "ola@work.com",
            EmailPrivate: "ola@private.com",
            MobilePhone: "912 34 567",
            Country: "NO",
            ActiveDirectoryLogin: "ola.ad",
            StreetAddress: "Gate 1",
            ZipCode: "0123",
            City: "Oslo",
            IdNumber: "12345678901",
            IdType: "NIN",
            IdCountry: "NO",
            EmploymentID: "EMP1",
            StartDate: "01.01.2020",
            EndDate: "",
            FTEFactor: "1.0",
            JobID: "42",
            OrgUnitId: "123",
            Location: "HQ",
            ManagerID: "",
            EmployeeID: "E1"
        );
}
