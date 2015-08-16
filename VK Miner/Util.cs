using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace VK_Miner
{
    internal static class Util
    {
        public static string GetNumEnding(this int number, params string[] endings)
        {
            number %= 100;

            if (number >= 11 && number <= 19)
                return endings[2];

            number %= 10;
            if (number == 1)
                return endings[0];
            if (number == 2 || number == 3 || number == 4)
                return endings[1];

            return endings[2];
        }

        public static string CamelCaseToSnakeCase(this string text)
        {
            return Regex.Replace(text, @"(\p{Ll})([\p{Lu}\d])", @"$1_$2").ToLower();
        }

        public static int ToInt(this string str) => int.Parse(str);
    }
}
