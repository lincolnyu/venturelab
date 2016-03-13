using System.Collections;
using System.Collections.Generic;

namespace GaussianCore.Helpers
{
    public static class ListHelper
    {
        public static void ReAlloc<T>(this List<T> list, int count)
        {
            list.Clear();
            list.Capacity = count;
        }

        public static void ReAllocAndInit<T>(this List<T> list, int count, T initValue=default(T))
        {
            ReAlloc(list, count);
            for (var i = 0; i < count; i++)
            {
                list.Add(initValue);
            }
        }
    }
}
