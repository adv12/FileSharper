// Copyright (c) 2017 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

namespace FileSharperCore
{
    public class MatchResult
    {

        public MatchResultType Type
        {
            get;
            private set;
        }

        public string[] Values
        {
            get;
            private set;
        }

        public MatchResult(MatchResultType type, params string[] values)
        {
            Type = type;
            Values = values;
        }
    }
}
