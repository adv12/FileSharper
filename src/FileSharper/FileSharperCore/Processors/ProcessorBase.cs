// Copyright (c) 2017 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System;
using System.IO;
using System.Threading;

namespace FileSharperCore.Processors
{
    public abstract class ProcessorBase : PluggableItemBase, IProcessor
    {

        public InputFileSource InputFileSource { get; set; }

        public abstract ProcessingResult Process(FileInfo originalFile, string[] values,
            FileInfo[] generatedFiles, ProcessInput whatToProcess,
            IProgress<ExceptionInfo> exceptionProgress, CancellationToken token);

        public virtual void ProcessAggregated(IProgress<ExceptionInfo> exceptionProgress,
            CancellationToken token)
        {

        }
    }
}
