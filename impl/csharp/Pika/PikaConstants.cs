using System.Text.RegularExpressions;

namespace Pika;

public static partial class PikaConstants
{
    public const ulong DefaultEpoch = 1640995200000UL;

    [GeneratedRegex("^[a-z0-9_]+$", RegexOptions.IgnoreCase, "en-US")]
    public static partial Regex ValidPrefixRegex();
}