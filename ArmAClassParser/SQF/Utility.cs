using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArmAClassParser.SQF
{
    public class Utility
    {
        /// <summary>
        /// Parses provided string in SQF string format to normal string.
        /// </summary>
        /// <param name="s">SQF-Formatted string</param>
        /// <returns>Normal formatted string</returns>
        public static string FromSqfString(string s)
        {
            s = s.Substring(1, s.Length - 2);
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i];
                if (c == '"')
                {
                    if (s[i + 1] == '"')
                        i++;
                    builder.Append(c);
                }
                else
                {
                    builder.Append(c);
                }
            }
            return builder.ToString();
        }
        /// <summary>
        /// Parses provided string into SQF-Formatted string.
        /// </summary>
        /// <param name="s">Normal formatted string<param>
        /// <returns>SQF-Formatted string</returns>
        public static string ToSqfString(string s, bool outerStringSpecifier = true)
        {
            StringBuilder builder = new StringBuilder(s.Length + 2);
            if (outerStringSpecifier)
                builder.Append('"');
            foreach (var c in s)
            {
                switch (c)
                {
                    case '"':
                        builder.Append("\"\"");
                        break;
                    default:
                        builder.Append(c);
                        break;
                    case '\r':
                    case '\n':
                        //Ignore theese chars
                        break;
                }
            }
            if (outerStringSpecifier)
                builder.Append('"');
            return builder.ToString();
        }
    }
}
