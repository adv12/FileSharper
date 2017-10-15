// Copyright (c) 2017 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileSharperCore
{
    public class MatchResult
    {
        private MatchResultType _type;
        private string[] _values;

        public MatchResultType Type
        {
            get
            {
                return _type;
            }
        }

        public string[] Values
        {
            get
            {
                return _values;
            }
        }

        public MatchResult(MatchResultType type, string[] values)
        {
            _type = type;
            _values = values;
        }
    }
}
