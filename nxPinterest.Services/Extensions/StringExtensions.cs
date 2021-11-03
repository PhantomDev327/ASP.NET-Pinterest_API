using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace nxPinterest.Services.Extensions
{
    public static class StringExtensions
    {
        public static string TrimExtraSpaces(this string input) {
            string result = Regex.Replace(input, @"\s{2,}", " ", RegexOptions.Multiline);
            return result;
        }
    }
}
