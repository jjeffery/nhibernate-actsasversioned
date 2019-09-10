using System.Text.RegularExpressions;
using JetBrains.Annotations;

namespace NHibernate.ActsAsVersioned.Internal
{
    /// <summary>
    /// Performs string manipulation for upper/lower case.
    /// </summary>
    public static class Inflector
    {
        public static string ToSnakeCase([NotNull] string s)
        {
            // UpperCase => Upper_Case
            string snakeCase = Regex.Replace(s, "([^A-Z_])([A-Z]+)", "$1_$2");
            snakeCase = snakeCase.ToLower();
            return snakeCase;
        }
    }
}
