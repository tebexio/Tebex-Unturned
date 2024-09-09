using System.Collections.Generic;

namespace Tebex.Util
{
    /// <summary>
    /// Utility class to represent UX friendly strings and the user intention.
    /// </summary>
    public class Intent
    {
        /// <summary>
        /// Set of strings that resolve to true
        /// </summary>
        private static readonly HashSet<string> TruthyStrings = new HashSet<string>
        {
            "true", "yes", "on", "1", "enabled", "enable"
        };

        /// <summary>
        /// Set of strings that resolve to false
        /// </summary>
        private static readonly HashSet<string> FalsyStrings = new HashSet<string>
        {
            "false", "no", "off", "0", "disabled", "disable"
        };

        public static bool IsTruthy(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return false;
            }

            return TruthyStrings.Contains(input.Trim().ToLowerInvariant());
        }

        public static bool IsFalsy(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return true;
            }

            return FalsyStrings.Contains(input.Trim().ToLowerInvariant());
        }
    }
}