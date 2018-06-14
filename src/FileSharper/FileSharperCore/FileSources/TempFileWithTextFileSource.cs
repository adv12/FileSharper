// Copyright (c) 2018 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using FileSharperCore.Editors;

namespace FileSharperCore.FileSources
{
    public class TempFileWithTextParameters
    {
        [Editor(typeof(FileSharperMultiLineTextEditor), typeof(FileSharperMultiLineTextEditor))]
        public string TempFileText { get; set; } = string.Empty;
    }

    public class TempFileWithTextFileSource : FileSourceBase
    {
        private TempFileWithTextParameters m_Parameters = new TempFileWithTextParameters();

        public override string Category => "Miscellaneous";

        public override string Name => "Single Temp File With Text";

        public override string Description => "Creates a temp file with the specified text as its content and returns that file.";

        public override object Parameters => m_Parameters;

        public override IEnumerable<FileInfo> Files
        {
            get
            {
                string tempFilePath = Path.GetTempFileName();
                File.WriteAllText(tempFilePath, m_Parameters.TempFileText);
                yield return new FileInfo(tempFilePath);
            }
        }
    }
}
