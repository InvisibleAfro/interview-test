namespace HrmFileImport.Options
{
    public sealed class SftpOptions
    {
        public string Host { get; init; } = "";
        public int Port { get; init; }
        public string Username { get; init; } = "";
        public string Password { get; init; } = "";
        public string Directory { get; init; } = "";
    }
}
