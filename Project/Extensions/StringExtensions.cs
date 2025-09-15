using System.Text.RegularExpressions;

namespace Project.Extensions
{
    public static class StringExtensions
    {
        public static string ToSnakeCase(this string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }

            var startUnderscores = Regex.Match(input, "^_+?");
            return startUnderscores + Regex.Replace(input, "([A-Z])", "_$1").ToLower().TrimStart('_');
        }
    }
}