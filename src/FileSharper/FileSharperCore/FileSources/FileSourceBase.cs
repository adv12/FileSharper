// Copyright (c) 2017 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System.Collections.Generic;
using System.IO;

namespace FileSharperCore.FileSources
{
    public abstract class FileSourceBase : PluggableItemBase, IFileSource
    {
        public abstract IEnumerable<FileInfo> Files
        {
            get;
        }
    }
}
