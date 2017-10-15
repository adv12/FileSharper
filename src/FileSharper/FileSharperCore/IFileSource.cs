// Copyright (c) 2017 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System.Collections.Generic;
using System.IO;

namespace FileSharperCore
{
    public interface IFileSource : IPluggableItem
    {
        IEnumerable<FileInfo> Files
        {
            get;
        }
    }
}
