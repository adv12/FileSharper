// Copyright (c) 2017 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace FileSharperCore.Conditions
{
    public abstract class ConditionBase : PluggableItemWithColumnsAndCacheTypesBase, ICondition
    {
        public override int ColumnCount => 0;

        public override string[] ColumnHeaders => new string[0];

        public abstract MatchResult Matches(FileInfo file, Dictionary<Type, IFileCache> fileCaches, CancellationToken token);
    }
}
