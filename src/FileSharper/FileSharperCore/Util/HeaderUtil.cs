// Copyright (c) 2017 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System.Collections.Generic;

namespace FileSharperCore.Util
{
    public class HeaderUtil
    {
        public static string[] GetUniqueHeaders(List<string> uniqueHeaders, ICondition condition, IFieldSource[] fieldSources)
        {
            List<string> headers = new List<string>();
            HashSet<string> usedHeaders = new HashSet<string>(uniqueHeaders);
            if (condition != null)
            {
                headers.AddRange(condition.ColumnHeaders);
            }
            if (fieldSources != null)
            {
                foreach (IFieldSource fieldSource in fieldSources)
                {
                    headers.AddRange(fieldSource.ColumnHeaders);
                }
            }
            foreach (string header in headers)
            {
                int num = 2;
                string newHeader = header;
                while (usedHeaders.Contains(newHeader))
                {
                    newHeader = header + " " + num;
                    num++;
                }
                uniqueHeaders.Add(newHeader);
                usedHeaders.Add(newHeader);
            }
            return uniqueHeaders.ToArray();
        }
    }
}
