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
    public class WordCountFieldSource : FieldSourceBase
    {
        public override int ColumnCount => 1;

        public override string[] ColumnHeaders => new string[] { "Word Count" };

        public override string Name => "Word Count";

        public override string Category => "Text";

        public override string Description => "The number of words in the text file";

        public override object Parameters => null;

        public override string[] GetValues(FileInfo file, Dictionary<Type, IFileCache> fileCaches, CancellationToken token)
        {
            int wordCount = 0;
            string value = "N/A";
            try
            {
                using (StreamReader reader = new StreamReader(file.FullName))
                {
                    wordCount = TextUtil.GetWordCount(reader, token);
                }
                value = wordCount.ToString();
            }
            catch (Exception ex)
            {

            }
            return new string[] { value };
        }
    }
}
