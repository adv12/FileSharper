// Copyright (c) 2017 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System.Threading;
using System.IO;
using System;
using System.Collections.Generic;

namespace FileSharperCore
{
    public interface ICondition : IPluggableItemWithColumnsAndCacheTypes
    {
        MatchResult Matches(FileInfo file, Dictionary<Type, IFileCache> fileCaches, CancellationToken token);
    }
}
