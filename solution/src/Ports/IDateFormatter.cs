namespace HrmFileImport.Ports
{
    public interface IDateFormatter
    {
        /// <summary>
        /// Converts dd.MM.yyyy → yyyy-MM-ddT00:00:00+0000
        /// Returns null if input is null/whitespace.
        /// Throws FormatException if invalid format.
        /// </summary>
        string? ToIsoUtcMidnight(string? ddMmYyyy);

        /// <summary>
        /// Same as ToIsoUtcMidnight but returns null if blank.
        /// </summary>
        string? ToIsoUtcMidnightOrNull(string? ddMmYyyy);
    }
}
