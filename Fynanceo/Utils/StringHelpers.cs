// Helpers/StringHelpers.cs
namespace Fynanceo.Helpers
{
    public static class StringHelpers
    {
        public static string Truncate(this string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return value.Length <= maxLength ? value : value.Substring(0, maxLength) + "...";
        }

        public static string FormatarMoeda(this decimal value)
        {
            return value.ToString("C");
        }

        public static string FormatarData(this DateTime value)
        {
            return value.ToString("dd/MM/yyyy HH:mm");
        }
    }
}