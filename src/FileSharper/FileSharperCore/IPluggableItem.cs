// Copyright (c) 2017 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

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

        void Init(RunInfo inf);

        void Cleanup();

    }
}
