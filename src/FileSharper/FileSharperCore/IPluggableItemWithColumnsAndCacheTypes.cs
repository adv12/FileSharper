// Copyright (c) 2017 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System;

namespace FileSharperCore
{
    public interface IPluggableItemWithColumnsAndCacheTypes: IPluggableItemWithColumns
    {
        Type[] CacheTypes { get; }
    }
}
