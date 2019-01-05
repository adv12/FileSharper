// Copyright (c) 2017 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Reflection;

namespace FileSharperCore
{
    public abstract class PluggableItemBase : MarshalByRefObject, IPluggableItem
    {

        protected IRunInfo RunInfo
        {
            get;
            private set;
        }

        protected CancellationToken CancellationToken
        {
            get => RunInfo.CancellationToken;
        }

        protected IProgress<IEnumerable<ExceptionInfo>> ExceptionProgress
        {
            get => RunInfo.ExceptionProgress;
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

        public void Init(IRunInfo inf)
        {
            RunInfo = inf;
            LocalInit();
        }

        public virtual void LocalInit()
        {

        }

        public void SetParameter(string name, object value)
        {
            object parameters = Parameters;
            Type t = parameters.GetType();
            PropertyInfo property = t.GetProperty(name);
            property.SetValue(parameters, value);
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
