// Caminho sugerido: Fynanceo/Utils/StringUtils.cs
using System.Text;
using System.Text.RegularExpressions;

namespace Fynanceo.Utils
{
    public static class StringUtils
    {
        /// <summary>
        /// Remove acentos e caracteres especiais de uma string.
        /// </summary>
        public static string RemoverCaracteresEspeciais(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            // Remove acentos
            string normalized = input.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();
            foreach (var c in normalized)
            {
                var unicodeCategory = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != System.Globalization.UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(c);
                }
            }

            string semAcentos = sb.ToString().Normalize(NormalizationForm.FormC);

            // Remove caracteres que não sejam letras, números ou espaço
            string resultado = Regex.Replace(semAcentos, @"[^0-9a-zA-Z\s]", string.Empty);

            return resultado;
        }
    }
}
