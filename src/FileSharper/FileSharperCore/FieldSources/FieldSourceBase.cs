// Copyright (c) 2017 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace FileSharperCore.FieldSources
{
    public abstract class FieldSourceBase : PluggableItemWithColumnsAndCacheTypesBase, IFieldSource
    {
        public abstract string[] GetValues(FileInfo file, Dictionary<Type, IFileCache> fileCaches, CancellationToken token);
    }
}
