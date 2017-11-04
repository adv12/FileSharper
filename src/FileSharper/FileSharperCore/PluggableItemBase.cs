// Copyright (c) 2017 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System;
using System.Threading;

namespace FileSharperCore
{
    public abstract class PluggableItemBase : MarshalByRefObject, IPluggableItem
    {
        private RunInfo m_RunInfo = null;

        protected RunInfo RunInfo
        {
            get
            {
                return m_RunInfo;
            }
            private set
            {
                m_RunInfo = value;
            }
        }

        protected CancellationToken CancellationToken
        {
            get => this.RunInfo.CancellationToken;
        }

        protected IProgress<ExceptionInfo> ExceptionProgress
        {
            get => this.RunInfo.ExceptionProgress;
        }

        public virtual string Category
        {
            get
            {
                return "Miscellaneous";
            }
        }

        public abstract string Name
        {
            get;
        }

        public abstract string Description
        {
            get;
        }

        public abstract object Parameters
        {
            get;
        }

        public void Init(RunInfo inf, IProgress<ExceptionInfo> exceptionProgress)
        {
            RunInfo = inf;
            LocalInit(exceptionProgress);
        }

        public virtual void LocalInit(IProgress<ExceptionInfo> exceptionProgress)
        {

        }

        public void Cleanup(IProgress<ExceptionInfo> exceptionProgress)
        {
            LocalCleanup(exceptionProgress);
            RunInfo = null;
        }

        public virtual void LocalCleanup(IProgress<ExceptionInfo> exceptionProgress)
        {

        }
    }
}
