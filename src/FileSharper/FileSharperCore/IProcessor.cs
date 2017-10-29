// Copyright (c) 2017 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System;
using System.IO;
using System.Threading;

namespace FileSharperCore
{
    public interface IProcessor: IPluggableItem
    {
        InputFileSource InputFileSource { get; set; }
        HowOften ProducesFiles { get; }
        ProcessingResult Process(FileInfo originalFile, string[] values,
            FileInfo[] generatedFiles, ProcessInput whatToProcess,
            IProgress<ExceptionInfo> exceptionProgress, CancellationToken token);
        void ProcessAggregated(IProgress<ExceptionInfo> exceptionProgress, CancellationToken token);
    }
}
