// Copyright (c) 2017 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using FileSharperCore.Util;

namespace FileSharperCore.FieldSources.Text
{
    public class LineEndingsFieldSource : FieldSourceBase
    {
        public override int ColumnCount => 1;

        public override string[] ColumnHeaders => new string[] { "Line Endings" };

        public override string Name => "Line Endings";

        public override string Category => "Text";

        public override string Description => "The type of line endings used in the text file";

        public override object Parameters => null;

        public override string[] GetValues(FileInfo file, Dictionary<Type, IFileCache> fileCaches, CancellationToken token)
        {
            DetectedLineEndings lineEndings = DetectedLineEndings.NotApplicable;
            string value = "N/A";
            try
            {
                using (StreamReader reader = new StreamReader(file.FullName))
                {
                    lineEndings = TextUtil.GetLineEndings(reader, false, token);
                }
                value = lineEndings.ToString();
            }
            catch (Exception ex)
            {
                
            }
            return new string[] { value };
        }
    }
}
