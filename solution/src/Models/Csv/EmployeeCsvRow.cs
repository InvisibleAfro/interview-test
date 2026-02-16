namespace HrmFileImport.Models.Csv
{
    public sealed record EmployeeCsvRow(
        string? FirstName,
        string? LastName,
        string? DateOfBirth,
        string? Gender,
        string? EmailWork,
        string? EmailPrivate,
        string? MobilePhone,
        string? Country,
        string? ActiveDirectoryLogin,
        string? StreetAddress,
        string? ZipCode,
        string? City,
        string? IdNumber,
        string? IdType,
        string? IdCountry,
        string? EmploymentID,
        string? StartDate,
        string? EndDate,
        string? FTEFactor,
        string? JobID,
        string? OrgUnitId,
        string? Location,
        string? ManagerID,
        string? EmployeeID
    );
}
