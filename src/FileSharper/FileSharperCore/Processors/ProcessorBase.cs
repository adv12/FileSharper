// Copyright (c) 2017 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System.IO;
using System.Threading;

namespace FileSharperCore.Processors
{
    public abstract class ProcessorBase : PluggableItemBase, IProcessor
    {
        public virtual HowOften ProducesFiles { get => HowOften.Never; }

        public bool ChainFromPrevious { get; set; }

        public abstract ProcessingResult Process(FileInfo originalFile, string[] values, FileInfo[] filesFromPrevious, CancellationToken token);

        public virtual void ProcessAggregated(CancellationToken token)
        {

        }
    }
}
