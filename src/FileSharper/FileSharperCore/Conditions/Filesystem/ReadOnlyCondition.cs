// Copyright (c) 2017 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace FileSharperCore.Conditions.Filesystem
{
    public class ReadOnlyCondition : ConditionBase
    {
        public override string Name => "Read Only";

        public override string Description => "File is read-only";

        public override string Category => "Filesystem";

        public override object Parameters => null;

        public override int ColumnCount => 1;

        public override string[] ColumnHeaders => new string[] { "ReadOnly" };

        public override MatchResult Matches(FileInfo file, Dictionary<Type, IFileCache> fileCaches, CancellationToken token)
        {
            string[] outputs = new string[] { file.IsReadOnly.ToString() };
            if (file.IsReadOnly)
            {
                return new MatchResult(MatchResultType.Yes, outputs);
            }
            else
            {
                return new MatchResult(MatchResultType.No, outputs);
            }
        }
    }
}
