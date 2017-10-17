// Copyright (c) 2017 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System.IO;
using System.Threading;

namespace FileSharperCore
{
    public interface IProcessor: IPluggableItem
    {
        bool ChainFromPrevious { get; set; }
        bool ProducesFiles { get; }
        ProcessingResult Process(FileInfo file, string[] values, FileInfo[] filesFromPrevious, CancellationToken token);
    }
}
