using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shift
{

    public static class IEnumerableExtensions
    {

        public static TSource AggregateSafe<TSource>(this IEnumerable<TSource> source, TSource backup, Func<TSource, TSource, TSource> func)
        {

            if(!source.Any())
            {
                return backup;
            }

            return source.Aggregate(func);

        }

    }

}
