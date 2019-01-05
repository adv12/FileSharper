// Copyright (c) 2017 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace FileSharperCore
{
    public class RunInfo : IRunInfo
    {
        private object m_Mutex = new object();

        public IFileSource FileSource
        {
            get;
            private set;
        }

        public ICondition Condition
        {
            get;
            private set;
        }

        public IFieldSource[] FieldSources
        {
            get;
            private set;
        }

        public IProcessor[] TestedProcessors
        {
            get;
            private set;
        }

        public IProcessor[] MatchedProcessors
        {
            get;
            private set;
        }

        public int MaxToMatch
        {
            get;
            private set;
        }

        public CancellationToken CancellationToken
        {
            get;
            private set;
        }

        public IProgress<string> FileSourceProgress
        {
            get;
            private set;
        }

        public ConcurrentQueue<FileProgressInfo> TestedFileProgressInfos
        {
            get;
            private set;
        }

        public ConcurrentQueue<FileProgressInfo> MatchedFileProgressInfos
        {
            get;
            private set;
        }

        public ConcurrentQueue<ExceptionInfo> ExceptionInfos
        {
            get;
            private set;
        }

        public IProgress<IEnumerable<FileProgressInfo>> TestedProgress
        {
            get;
            private set;
        }
        
        public IProgress<IEnumerable<FileProgressInfo>> MatchedProgress
        {
            get;
            private set;
        }

        public IProgress<IEnumerable<ExceptionInfo>> ExceptionProgress
        {
            get;
            private set;
        }

        public IProgress<bool> CompleteProgress
        {
            get;
            private set;
        }

        private bool m_StopRequested;
        public bool StopRequested
        {
            get
            {
                lock (m_Mutex)
                {
                    return m_StopRequested;
                }
            }
            private set
            {
                lock (m_Mutex)
                {
                    m_StopRequested = value;
                }
            }
        }

        public RunInfo(IFileSource fileSource, ICondition condition,
            IFieldSource[] fieldSources, IProcessor[] testedProcessors,
            IProcessor[] matchedProcessors, int maxToMatch, CancellationToken token,
            IProgress<string> fileSourceProgress,
            IProgress<IEnumerable<FileProgressInfo>> testedProgress,
            IProgress<IEnumerable<FileProgressInfo>> matchedProgress,
            IProgress<IEnumerable<ExceptionInfo>> exceptionProgress,
            IProgress<bool> completeProgress)
        {
            FileSource = fileSource;
            Condition = condition;
            FieldSources = fieldSources;
            TestedProcessors = testedProcessors;
            MatchedProcessors = matchedProcessors;
            MaxToMatch = maxToMatch == 0 ? -1 : maxToMatch;
            CancellationToken = token;
            FileSourceProgress = fileSourceProgress;
            TestedProgress = testedProgress;
            MatchedProgress = matchedProgress;
            ExceptionProgress = exceptionProgress;
            CompleteProgress = completeProgress;

            TestedFileProgressInfos = new ConcurrentQueue<FileProgressInfo>();
            MatchedFileProgressInfos = new ConcurrentQueue<FileProgressInfo>();
            ExceptionInfos = new ConcurrentQueue<ExceptionInfo>();
        }

        public void RequestStop()
        {
            StopRequested = true;
        }
    }
}
