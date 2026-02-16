using HrmFileImport.Ports;
using System.Globalization;

namespace HrmFileImport.Mappers
{
    public sealed class DateFormatter : IDateFormatter
    {
        private const string InputFormat = "dd.MM.yyyy";

        public string? ToIsoUtcMidnight(string? ddMmYyyy)
        {
            if (string.IsNullOrWhiteSpace(ddMmYyyy))
                return null;

            var trimmed = ddMmYyyy.Trim();

            if (!DateTime.TryParseExact(
                    trimmed,
                    InputFormat,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var date))
            {
                throw new FormatException(
                    $"Invalid date '{trimmed}'. Expected format {InputFormat}.");
            }

            return $"{date:yyyy-MM-dd}T00:00:00+0000";
        }

        public string? ToIsoUtcMidnightOrNull(string? ddMmYyyy)
        {
            if (string.IsNullOrWhiteSpace(ddMmYyyy))
                return null;

            return ToIsoUtcMidnight(ddMmYyyy);
        }
    }
}
