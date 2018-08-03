// Copyright (c) 2017 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System;
using System.Collections.Generic;
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

        protected IProgress<IEnumerable<ExceptionInfo>> ExceptionProgress
        {
            get => this.RunInfo.ExceptionProgress;
        }

        public abstract string Category
        {
            get;
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

        public void Init(RunInfo inf)
        {
            RunInfo = inf;
            LocalInit();
        }

        public virtual void LocalInit()
        {

        }

        public void Cleanup()
        {
            LocalCleanup();
            RunInfo = null;
        }

        public virtual void LocalCleanup()
        {

        }
    }
}
