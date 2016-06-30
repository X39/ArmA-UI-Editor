using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArmAClassParser.SQF
{
    public static class Utility
    {
        /// <summary>
        /// Parses provided string in SQF string format to normal string.
        /// </summary>
        /// <param name="s">SQF-Formatted string</param>
        /// <returns>Normal formatted string</returns>
        public static string FromSqfString(this string s)
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
        public static string ToSqfString(this string s, bool outerStringSpecifier = true)
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
        /// <summary>
        /// Creates a new array containing all items from startIndex to length in thisArray
        /// </summary>
        /// <param name="thisArray">Array to get the range from</param>
        /// <param name="startIndex">index where to start</param>
        /// <param name="length">length of the items to receive or -1</param>
        /// <returns></returns>
        public static string[] GetRange(this string[] thisArray, int startIndex, int length = -1)
        {
            string[] outArray = new string[length == -1 ? thisArray.Length - startIndex : length];
            int outArrayIndex = 0;
            for(int i = startIndex; i < (length == -1 ? thisArray.Length - startIndex : length); i++)
            {
                outArray[outArrayIndex++] = thisArray[i];
            }
            return outArray;
        }
    }
}
