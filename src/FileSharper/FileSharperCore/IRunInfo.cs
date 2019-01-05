// Copyright (c) 2019 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace FileSharperCore
{
    public interface IRunInfo
    {
        IFileSource FileSource { get; }

        ICondition Condition
        {
            get;
        }

        IFieldSource[] FieldSources
        {
            get;
        }

        IProcessor[] TestedProcessors
        {
            get;
        }

        IProcessor[] MatchedProcessors
        {
            get;
        }

        int MaxToMatch
        {
            get;
        }

        CancellationToken CancellationToken
        {
            get;
        }

        IProgress<string> FileSourceProgress
        {
            get;
        }

        ConcurrentQueue<FileProgressInfo> TestedFileProgressInfos
        {
            get;
        }

        ConcurrentQueue<FileProgressInfo> MatchedFileProgressInfos
        {
            get;
        }

        ConcurrentQueue<ExceptionInfo> ExceptionInfos
        {
            get;
        }

        IProgress<IEnumerable<FileProgressInfo>> TestedProgress
        {
            get;
        }

        IProgress<IEnumerable<FileProgressInfo>> MatchedProgress
        {
            get;
        }

        IProgress<IEnumerable<ExceptionInfo>> ExceptionProgress
        {
            get;
        }

        IProgress<bool> CompleteProgress
        {
            get;
        }

        bool StopRequested
        {
            get;
        }
    }
}
