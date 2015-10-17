using System.Collections.Generic;

namespace CrossWord
{
    public static class CollectionsExtensions
    {
        public static T Back<T>(this List<T> list)
        {
            return list[list.Count - 1];
        }

        public static void Pop<T>(this List<T> list)
        {
            list.RemoveAt(list.Count - 1);
        }
    }
}
