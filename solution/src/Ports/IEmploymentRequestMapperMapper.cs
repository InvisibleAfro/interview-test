using HrmFileImport.Models.Csv;
using HrmFileImport.Models.HrmApi;

namespace HrmFileImport.Ports
{
    public interface IEmploymentRequestMapperMapper
    {
        CreateEmploymentRequestDto Map(EmployeeCsvRow row);
    }
}
