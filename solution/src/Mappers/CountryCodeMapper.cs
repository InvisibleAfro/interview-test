using HrmFileImport.Ports;

namespace HrmFileImport.Mappers
{
    public class CountryCodeMapper : ICountryCodeMapper
    {
        private readonly Dictionary<string, string> Alpha2ToAlpha3 =
            new(StringComparer.OrdinalIgnoreCase)
            {
                ["NO"] = "NOR",
                ["SE"] = "SWE",
                ["DK"] = "DNK",
                ["FI"] = "FIN",
                ["GB"] = "GBR",
                ["US"] = "USA",
                ["DE"] = "DEU",
                ["NL"] = "NLD"
            };

        private readonly Dictionary<string, string> DialCodes =
            new(StringComparer.OrdinalIgnoreCase)
            {
                ["NO"] = "+47",
                ["SE"] = "+46",
                ["DK"] = "+45",
                ["FI"] = "+358",
                ["GB"] = "+44",
                ["US"] = "+1",
                ["DE"] = "+49",
                ["NL"] = "+31"
            };

        public string? ToAlpha3(string? alpha2)
        {
            if (string.IsNullOrWhiteSpace(alpha2))
                return null;

            var trimmed = alpha2.Trim();

            if (Alpha2ToAlpha3.TryGetValue(trimmed, out var alpha3))
                return alpha3;

            throw new KeyNotFoundException(
                $"No ISO alpha-3 mapping found for country code '{trimmed}'.");
        }

        public string? ToDialCode(string? alpha2)
        {
            if (string.IsNullOrWhiteSpace(alpha2))
                return null;

            var trimmed = alpha2.Trim();

            if (DialCodes.TryGetValue(trimmed, out var dialCode))
                return dialCode;

            throw new KeyNotFoundException(
                $"No dial code mapping found for country code '{trimmed}'.");
        }
    }
}
