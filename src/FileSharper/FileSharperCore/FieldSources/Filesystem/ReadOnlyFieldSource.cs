// Copyright (c) 2017 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace FileSharperCore.FieldSources.Filesystem
{
    public class ReadOnlyFieldSource : FieldSourceBase
    {
        public override int ColumnCount => 1;

        public override string[] ColumnHeaders => new string[] { "Read-Only" };

        public override string Name => "Read-Only";

        public override string Description => "Whether the file is read-only";

        public override string Category => "Filesystem";

        public override object Parameters => null;

        public override string[] GetValues(FileInfo file, Dictionary<Type, IFileCache> cacheTypes, CancellationToken token)
        {
            string field = file.IsReadOnly ? "Yes" : "No";
            string[] fields = new string[] { field };
            return fields;
        }
    }
}
