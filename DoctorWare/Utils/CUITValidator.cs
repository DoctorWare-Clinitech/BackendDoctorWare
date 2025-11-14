using System.Linq;
using System.Text.RegularExpressions;

namespace DoctorWare.Utils
{
    public static class CUITValidator
    {
        private static readonly Regex Digits11 = new Regex("^\\d{11}$", RegexOptions.Compiled);

        public static bool IsValidCUIT(string? cuit)
        {
            if (string.IsNullOrWhiteSpace(cuit))
            {
                return false;
            }
            string normalized = new string(cuit.Where(char.IsDigit).ToArray());
            if (!Digits11.IsMatch(normalized))
            {
                return false;
            }

            int[] weights = new int[] { 5, 4, 3, 2, 7, 6, 5, 4, 3, 2 };
            int sum = 0;
            for (int i = 0; i < 10; i++)
            {
                sum += (normalized[i] - '0') * weights[i];
            }
            int mod = sum % 11;
            int check = 11 - mod;
            if (check == 11)
            {
                check = 0;
            }
            if (check == 10)
            {
                check = 9; // criterio AFIP
            }
            return check == (normalized[10] - '0');
        }

        public static string Normalize(string cuit)
        {
            return new string(cuit.Where(char.IsDigit).ToArray());
        }
    }
}
