using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.Collections;

namespace Utility
{
    public static class IEnumerableExtensionMethods
    {
        #region FirstOrNull(able)
        public static T FirstOrNull<T>(this IEnumerable<T> enumerable, Func<T, bool> func) where T : class
        {
            foreach (var it in enumerable)
            {
                if (func(it))
                {
                    return it;
                }
            }
            return null;
        }
        public static T? FirstOrNullable<T>(this IEnumerable<T> enumerable, Func<T, bool> func) where T : struct
        {
            foreach (var it in enumerable)
            {
                if (func(it))
                {
                    return it;
                }
            }
            return null;
        }
        #endregion
        #region Pick
        public static IEnumerable<T> Pick<T>(this IEnumerable<T> enumerable, Func<T, bool> func)
        {
            var list = new List<T>();
            foreach (var it in enumerable)
            {
                if (func(it))
                {
                    list.Add(it);
                }
            }
            return list;
        }
        #endregion
    }
}
