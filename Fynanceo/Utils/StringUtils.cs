// Caminho sugerido: Fynanceo/Utils/StringUtils.cs
using System.Text;
using System.Text.RegularExpressions;

namespace Fynanceo.Utils
{
    public static class StringUtils
    {
        /// <summary>
        /// Remove acentos de uma string.
        /// </summary>
        public static string RemoverAcentos(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return text;

            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != System.Globalization.UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }

        public static string RemoverAcentosELower(string text)
        {
            return RemoverAcentos(text)?.ToLower();
        }

        /// <summary>
        /// Remove acentos e caracteres especiais de uma string.
        /// </summary>
        public static string RemoverCaracteresEspeciais(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            // Remove acentos
            string semAcentos = RemoverAcentos(input);

            // Remove caracteres que não sejam letras, números ou espaço
            string resultado = Regex.Replace(semAcentos, @"[^0-9a-zA-Z\s]", string.Empty);

            return resultado;
        }
        
        /// <summary>
        /// Formata CPF ou CNPJ automaticamente.
        /// Aceita qualquer entrada e remove caracteres não numéricos.
        /// </summary>
        public static string FormatarCpfCnpj(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            // Mantém somente números
            string valor = Regex.Replace(input, @"\D", "");

            if (valor.Length == 11)
            {
                // CPF -> 000.000.000-00
                return Convert.ToUInt64(valor).ToString(@"000\.000\.000\-00");
            }
            else if (valor.Length == 14)
            {
                // CNPJ -> 00.000.000/0000-00
                return Convert.ToUInt64(valor).ToString(@"00\.000\.000\/0000\-00");
            }

            return input; // Retorna original se não for CPF ou CNPJ
        }
        
        /// <summary>
        /// Formata número de telefone brasileiro.
        /// Exemplos:
        /// 11988884444 -> (11) 98888-4444
        /// 38884444    -> 3888-4444
        /// </summary>
        public static string FormataTelefone(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            // Mantém apenas números
            string numeros = Regex.Replace(input, @"\D", "");

            // Formato celular com DDD: 11 dígitos (ex: 11988884444)
            if (numeros.Length == 11)
            {
                return Regex.Replace(numeros, @"(\d{2})(\d{5})(\d{4})", "($1) $2-$3");
            }

            // Formato fixo com DDD: 10 dígitos (ex: 1138884444)
            if (numeros.Length == 10)
            {
                return Regex.Replace(numeros, @"(\d{2})(\d{4})(\d{4})", "($1) $2-$3");
            }

            // Formato sem DDD: 8 ou 9 dígitos
            if (numeros.Length == 9)
            {
                return Regex.Replace(numeros, @"(\d{5})(\d{4})", "$1-$2");
            }

            if (numeros.Length == 8)
            {
                return Regex.Replace(numeros, @"(\d{4})(\d{4})", "$1-$2");
            }

            // Se não bate nenhum formato esperado, retorna só os números
            return numeros;
        }

        /// <summary>
        /// Formata CEP no padrão XXXXX-XXX.
        /// </summary>
        public static string FormataCep(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            // Mantém apenas números
            string numeros = Regex.Replace(input, @"\D", "");

            if (numeros.Length == 8)
            {
                return Regex.Replace(numeros, @"(\d{5})(\d{3})", "$1-$2");
            }

            // Caso tenha menos ou mais, retorna apenas os números
            return numeros;
        }
    }
        
 
    }

