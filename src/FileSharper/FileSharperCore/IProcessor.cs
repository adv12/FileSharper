// Copyright (c) 2017 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace FileSharperCore
{
    public interface IProcessor: IPluggableItem
    {
        InputFileSource InputFileSource { get; set; }
        ProcessingResult Process(FileInfo originalFile, MatchResultType matchResultType, string[] values,
            FileInfo[] generatedFiles, ProcessInput whatToProcess,
            IList<ExceptionInfo> exceptionInfos, CancellationToken token);
        void ProcessAggregated(IList<ExceptionInfo> exceptionInfos, CancellationToken token);
    }
}
