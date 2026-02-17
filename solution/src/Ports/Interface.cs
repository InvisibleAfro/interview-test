using HrmFileImport.Models.HrmApi;

namespace HrmFileImport.Ports
{
    public interface IHrmApiClient
    {
        Task PostEmployee(CreateEmploymentRequestDto employmentRequestDto, string authToken, CancellationToken ct);
    }
}
