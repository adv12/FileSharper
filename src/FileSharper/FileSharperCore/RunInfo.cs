// Copyright (c) 2017 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System;
using System.Threading;

namespace FileSharperCore
{
    public class RunInfo
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

        public IOutput[] Outputs
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

        public IProgress<FileProgressInfo> TestedProgress
        {
            get;
            private set;
        }
        
        public IProgress<FileProgressInfo> MatchedProgress
        {
            get;
            private set;
        }

        public IProgress<ExceptionInfo> ExceptionProgress
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
            IOutput[] outputs, IProcessor[] testedProcessors,
            IProcessor[] matchedProcessors, int maxToMatch, CancellationToken token,
            IProgress<FileProgressInfo> testedProgress, IProgress<FileProgressInfo> matchedProgress,
            IProgress<ExceptionInfo> exceptionProgress, IProgress<bool> completeProgress)
        {
            FileSource = fileSource;
            Condition = condition;
            Outputs = outputs;
            TestedProcessors = testedProcessors;
            MatchedProcessors = matchedProcessors;
            MaxToMatch = maxToMatch == 0 ? -1 : maxToMatch;
            CancellationToken = token;
            TestedProgress = testedProgress;
            MatchedProgress = matchedProgress;
            ExceptionProgress = exceptionProgress;
            CompleteProgress = completeProgress;
        }

        public void RequestStop()
        {
            StopRequested = true;
        }
    }
}
