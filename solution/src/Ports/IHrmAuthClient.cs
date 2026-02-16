namespace HrmFileImport.Ports
{
    public interface IHrmAuthClient
    {
        public Task<string> GetAccessTokenAsync(CancellationToken ct = default);
    }
}
