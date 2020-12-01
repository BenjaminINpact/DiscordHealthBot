using System;
using System.Collections.Generic;
using System.Linq;

namespace DiscordHealBot
{
    public static class LinqExtensions
    {
        public static IEnumerable<TSource> Except<TSource, VSource>(this IEnumerable<TSource> first, IEnumerable<VSource> second, Func<TSource, VSource, bool> comparer)
        {
            return first.Where(x => second.Count(y => comparer(x, y)) == 0);
        }

        public static  IEnumerable<TSource> Except<TSource, VSource>(this Dictionary<string, TSource> first, Dictionary<string, VSource> second)
        {
            return first.AsParallel().Where(x => !second.ContainsKey(x.Key)).Select(x => x.Value).ToList();
       
        }

        public static IEnumerable<TSource> Contains<TSource, VSource>(this IEnumerable<TSource> first, IEnumerable<VSource> second, Func<TSource, VSource, bool> comparer)
        {
            return first.Where(x => second.FirstOrDefault(y => comparer(x, y)) != null);
        }

        public static IEnumerable<TSource> Intersect<TSource, VSource>(this IEnumerable<TSource> first, IEnumerable<VSource> second, Func<TSource, VSource, bool> comparer)
        {
            return first.Where(x => second.Count(y => comparer(x, y)) == 1);
        }
        
        public static IEnumerable<TSource> DistinctBy<TSource, TKey>
            (this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            HashSet<TKey> seenKeys = new HashSet<TKey>();
            foreach (TSource element in source)
            {
                if (seenKeys.Add(keySelector(element)))
                {
                    yield return element;
                }
            }
        }
        
        
    }
}