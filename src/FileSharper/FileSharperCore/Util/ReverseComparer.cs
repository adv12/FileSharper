// Copyright (c) 2017 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System.Collections.Generic;

namespace FileSharperCore.Util
{
    public class ReverseComparer<T> : IComparer<T>
    {
        public IComparer<T> m_Comparer;

        public ReverseComparer(IComparer<T> comparer)
        {
            m_Comparer = comparer;
        }

        public int Compare(T x, T y)
        {
            return m_Comparer.Compare(y, x);
        }
    }
}
