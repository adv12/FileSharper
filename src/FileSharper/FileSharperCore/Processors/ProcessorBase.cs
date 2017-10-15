// Copyright (c) 2017 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System.IO;
using System.Threading;

namespace FileSharperCore.Processors
{
    public abstract class ProcessorBase : PluggableItemBase, IProcessor
    {
        public virtual bool ProducesFiles { get => false; }

        public bool ChainFromPrevious { get; set; }

        public abstract FileInfo[] Process(FileInfo file, string[] values, FileInfo[] filesFromPrevious, CancellationToken token);
    }
}
