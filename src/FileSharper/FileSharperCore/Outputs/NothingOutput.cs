// Copyright (c) 2017 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FileSharperCore.Outputs
{
    public class NothingOutput : OutputBase
    {
        public override int ColumnCount => 0;

        public override string[] ColumnHeaders => new string[0];

        public override string Name => null;

        public override string Category => string.Empty;

        public override string Description => null;

        public override object Parameters => null;

        public override string[] GetValues(FileInfo file, Dictionary<Type, IFileCache> cacheTypes, CancellationToken token)
        {
            return new string[0];
        }
    }
}
