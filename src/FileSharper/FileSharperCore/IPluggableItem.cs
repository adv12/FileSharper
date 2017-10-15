// Copyright (c) 2017 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System;

namespace FileSharperCore
{
    public interface IPluggableItem
    {
        object Parameters
        {
            get;
        }

        string Category
        {
            get;
        }

        string Name
        {
            get;
        }

        string Description
        {
            get;
        }

        void Init(RunInfo inf, IProgress<ExceptionInfo> exceptionProgress);

        void Cleanup(IProgress<ExceptionInfo> exceptionProgress);

    }
}
