namespace HrmFileImport.Ports
{
    public interface ICountryCodeMapper
    {
        string? ToAlpha3(string? alpha2);
        string? ToDialCode(string? alpha2);
    }
}
