// Copyright (c) 2017 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using FileSharperCore.Editors;
using FileSharperCore.Util;

namespace FileSharperCore.FileSources
{

    public class FileFileSourceParameters
    {
        [Editor(typeof(OpenFileEditor), typeof(OpenFileEditor))]
        public string File { get; set; }
    }

    public class FileFileSource : FileSourceBase
    {

        private FileFileSourceParameters m_Parameters = new FileFileSourceParameters();
        
        public override string Name => "File";

        public override string Description => "Reads full file paths from a text file";

        public override object Parameters => m_Parameters;

        public override IEnumerable<FileInfo> Files
        {
            get {
                string file = ReplaceUtil.Replace(m_Parameters.File, (FileInfo)null);
                using (StreamReader reader = new StreamReader(file))
                {
                    while (!reader.EndOfStream)
                    {
                        if (RunInfo.StopRequested)
                        {
                            yield break;
                        }
                        RunInfo.CancellationToken.ThrowIfCancellationRequested();
                        string line = reader.ReadLine().Trim();
                        line = ReplaceUtil.Replace(line, (FileInfo)null);
                        yield return new FileInfo(line);
                    }
                }
            }
        }

    }
}
