using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xandevelop.Wigwam.Compiler.Extensions
{
    static class ListExns
    {
        public static T SafeGet<T>(this List<T> list, int ix)
        {
            if (list.Count > ix) return list[ix];
            return default(T); // null, probably
        } 
    }
}
