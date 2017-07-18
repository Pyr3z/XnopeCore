using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Xnope
{
    /// <summary>
    /// Various utilities concerning IEnumerables.
    /// </summary>
    public static class EnumerableUtils
    {
        public static IEnumerable<T> LimitBetween<T>(this IEnumerable<T> enumerable, int lowerBound, int upperBound)
        {
            int length = enumerable.Count();

            if (lowerBound < 0 || upperBound < 0 || lowerBound > length - 1 || upperBound > length - 1 || lowerBound > length)
            {
                Log.Warning("[XnopeCore] Tried to limit an enumerable between " + lowerBound + " and " + upperBound + ". Returning original enumerable.");
                foreach (var e in enumerable)
                {
                    yield return e;
                }
            }

            if (lowerBound > upperBound)
            {
                var temp1 = lowerBound;
                var temp2 = upperBound;
                upperBound = temp1;
                lowerBound = temp2;
            }

            var enu = enumerable.GetEnumerator();

            int i = 0;
            while (i++ < upperBound && enu.MoveNext())
            {
                if (i > lowerBound)
                {
                    yield return enu.Current;
                }
            }

        }

        /// <summary>
        /// Limits the number of elements in the IEnumerable to the integer specified.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumerable"></param>
        /// <param name="numElements"></param>
        /// <returns></returns>
        public static IEnumerable<T> LimitTo<T>(this IEnumerable<T> enumerable, int numElements)
        {
            if (numElements > enumerable.Count() || numElements < 1)
            {
                Log.Warning("[XnopeCore] Tried to limit an enumerable to an index out of bounds. Returning the original.");
                numElements = enumerable.Count();
            }

            var enu = enumerable.GetEnumerator();

            int i = 0;
            while (i++ < numElements && enu.MoveNext())
            {
                yield return enu.Current;
            }
        }

        public static IEnumerable<T> RandomGroup<T>(this IEnumerable<T> enumerable, int groupCount)
        {
            int length = enumerable.Count();

            if (groupCount < 0 || groupCount > length)
            {
                Log.Warning("[XnopeCore] Tried to get a random group in an enumerable with groupCount out of bounds. Returning the original.");
                foreach (var e in enumerable)
                {
                    yield return e;
                }
                yield break;
            }

            var enu = enumerable.GetEnumerator();

            int upperBound = length - groupCount;

            int rand = Rand.RangeInclusive(0, upperBound);

            int i = 0;
            int j = 0;
            while (enu.MoveNext())
            {
                if (i++ >= rand)
                {
                    if (j++ == groupCount)
                    {
                        break;
                    }
                    yield return enu.Current;
                }
            }
        }

    }
}
