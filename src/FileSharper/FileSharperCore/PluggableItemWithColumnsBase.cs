// Copyright (c) 2017 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

namespace FileSharperCore
{
    public abstract class PluggableItemWithColumnsBase : PluggableItemBase, IPluggableItemWithColumns
    {
        public abstract int ColumnCount
        {
            get;
        }

        public abstract string[] ColumnHeaders
        {
            get;
        }
    }
}
