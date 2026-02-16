namespace HrmFileImport.Ports
{
    public interface IEmployeeSource
    {
        Task<Stream> DownloadEmployeeSourceAsync(CancellationToken ct = default);
    }
}
