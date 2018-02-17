// Copyright (c) 2017 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace FileSharperCore.Util
{
    public class HeaderUtil
    {
        public static string[] GetUniqueHeaders(List<string> uniqueHeaders, ICondition condition,
            IFieldSource[] fieldSources, bool forBinding = false)
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
                string baseHeader = forBinding ? Regex.Replace(header, @"[^a-zA-Z0-9_\- ]", "X") : header;
                int num = 2;
                string newHeader = baseHeader;
                while (usedHeaders.Contains(newHeader))
                {
                    newHeader = baseHeader + " " + num;
                    num++;
                }
                uniqueHeaders.Add(newHeader);
                usedHeaders.Add(newHeader);
            }
            return uniqueHeaders.ToArray();
        }
    }
}
