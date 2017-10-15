// Copyright (c) 2017 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace FileSharperCore.Conditions
{
    public class MatchEverythingCondition : ConditionBase
    {
        public override string Name => null;

        public override string Description => null;

        public override string Category => string.Empty;

        public override object Parameters => null;

        public override MatchResult Matches(FileInfo file, Dictionary<Type, IFileCache> fileCaches, CancellationToken token)
        {
            return new MatchResult(MatchResultType.Yes, new string[0]);
        }
    }
}
