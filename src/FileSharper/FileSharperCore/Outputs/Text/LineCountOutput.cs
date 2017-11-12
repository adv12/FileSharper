// Copyright (c) 2017 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using FileSharperCore.Util;

namespace FileSharperCore.Outputs.Text
{
    public class LineCountOutput : OutputBase
    {
        public override int ColumnCount => 1;

        public override string[] ColumnHeaders => new string[] { "Line Count" };

        public override string Name => "Line Count";

        public override string Description => "The number of lines in the text file";

        public override string Category => "Text";

        public override object Parameters => null;

        public override string[] GetValues(FileInfo file, Dictionary<Type, IFileCache> fileCaches, CancellationToken token)
        {
            int lineCount = 0;
            string value = "N/A";
            try
            {
                using (StreamReader reader = new StreamReader(file.FullName))
                {
                    lineCount = TextUtil.GetLineCount(reader, token);
                }
                value = lineCount.ToString();
            }
            catch (Exception ex)
            {

            }
            return new string[] { value };
        }
    }
}
