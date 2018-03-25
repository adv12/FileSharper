// Copyright (c) 2018 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using FileSharperCore.Util;

namespace FileSharperCore.FieldSources.Text
{
    public class TextEncodingFieldSource : FieldSourceBase
    {
        public override int ColumnCount => 1;

        public override string[] ColumnHeaders => new string[] { "Encoding" };

        public override string Category => "Text";

        public override string Name => "Text Encoding";

        public override string Description => null;

        public override object Parameters => null;

        public override string[] GetValues(FileInfo file, Dictionary<Type, IFileCache> fileCaches, CancellationToken token)
        {
            string encoding = "N/A";
            try
            {
                encoding = TextUtil.GetDetectedEncodingType(file).ToString();
            }
            catch (Exception)
            {

            }
            return new string[] { encoding };
        }
    }
}
