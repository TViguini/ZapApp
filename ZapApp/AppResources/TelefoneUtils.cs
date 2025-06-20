using PhoneNumbers;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ZapApp.AppResources
{
    public static class TelefoneUtils
    {
        /// <summary>
        /// Extrai e valida SOMENTE NÚMEROS DE CELULAR de uma string de texto livre.
        /// Abordagem revisada usando Regex para encontrar candidatos e Parse para validar.
        /// </summary>
        /// <param name="entrada">A string de texto contendo um ou mais números.</param>
        /// <returns>Uma lista de números de CELULAR válidos no formato E.164 (+55DDDNMERO).</returns>
        public static List<string> ExtrairTelefonesCelulares(string entrada)
        {
            var telefonesValidos = new List<string>();
            if (string.IsNullOrWhiteSpace(entrada))
            {
                return telefonesValidos;
            }

            // 1. Regex para encontrar padrões que se parecem com números de telefone brasileiros.
            // Esta regex é mais "permissiva" para capturar vários formatos, incluindo DDD e o nono dígito.
            const string phonePattern = @"(?:\+?55\s*)?(?:\(?\d{2}\)?\s*)?(?:[9]\s*-?\s*\d{4,5}\s*-?\s*\d{4}|\d{4,5}\s*-?\s*\d{4})";
            var matches = Regex.Matches(entrada, phonePattern);

            var phoneUtil = PhoneNumberUtil.GetInstance();

            foreach (Match match in matches)
            {
                // 2. Trabalha com cada "candidato" a número encontrado pela Regex.
                string potentialNumber = match.Value;

                try
                {
                    // 3. Usa o 'Parse', que é feito para validar um único número.
                    // A região "BR" é crucial para ele entender DDD vs número local.
                    var numero = phoneUtil.Parse(potentialNumber, "BR");

                    // 4. Valida e filtra pelo tipo 'MOBILE'.
                    if (phoneUtil.IsValidNumber(numero))
                    {
                        var tipo = phoneUtil.GetNumberType(numero);
                        if (tipo == PhoneNumberType.MOBILE)
                        {
                            string numeroFormatado = phoneUtil.Format(numero, PhoneNumberFormat.E164);
                            telefonesValidos.Add(numeroFormatado);
                        }
                    }
                }
                catch (NumberParseException)
                {
                    // Se o Parse falhar (por ser um falso positivo da Regex), simplesmente ignora.
                    continue;
                }
            }

            return telefonesValidos.Distinct().ToList();
        }
    }
}