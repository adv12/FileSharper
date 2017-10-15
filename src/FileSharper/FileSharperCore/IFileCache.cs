// Copyright (c) 2017 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System;
using System.IO;

namespace FileSharperCore
{
    public interface IFileCache : IDisposable
    {
        void Load(FileInfo file);
    }
}
