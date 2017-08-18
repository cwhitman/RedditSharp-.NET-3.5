using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RedditSharp.Utils
{
    /// <summary>
    /// Contains utility methods for Enums that exist in versions of .NET higher than 3.5.
    /// </summary>
    class EnumUtils
    {
        /// <summary>
        /// Attempts to parse the value into an Enum.
        /// </summary>
        /// <typeparam name="TEnum">The Enum to be parses into.</typeparam>
        /// <param name="value">A string representing the Enum.</param>
        /// <param name="result">The parsed Enum.</param>
        /// <returns>True if the parse is successful, false otherwise.</returns>
        public static bool TryParse<TEnum>(string value,bool ignoreCase, out TEnum result)
    where TEnum : struct, IConvertible
        {
            bool parsed = false;
            try
            {
                result = (TEnum)Enum.Parse(typeof(TEnum), value, ignoreCase);
                parsed = true;
            }
            catch {
                result = default(TEnum);
            }

            return parsed;
        }

        /// <summary>
        /// Check to see if a flags enumeration has a specific flag set.
        /// </summary>
        /// <param name="variable">Flags enumeration to check</param>
        /// <param name="value">Flag to check for</param>
        /// <returns></returns>
        public static bool HasFlag(Enum variable, Enum value)
        {
            if (variable == null)
                return false;

            if (value == null)
                throw new ArgumentNullException("value");

            // Not as good as the .NET 4 version of this function, but should be good enough
            if (!Enum.IsDefined(variable.GetType(), value))
            {
                throw new ArgumentException(string.Format(
                    "Enumeration type mismatch.  The flag is of type '{0}', was expecting '{1}'.",
                    value.GetType(), variable.GetType()));
            }

            ulong num = Convert.ToUInt64(value);
            return ((Convert.ToUInt64(variable) & num) == num);

        }

    }
}
