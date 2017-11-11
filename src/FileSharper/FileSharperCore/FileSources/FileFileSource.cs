using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using FileSharperCore.Editors;

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
                using (StreamReader reader = new StreamReader(m_Parameters.File))
                {
                    while (!reader.EndOfStream)
                    {
                        if (RunInfo.StopRequested)
                        {
                            yield break;
                        }
                        RunInfo.CancellationToken.ThrowIfCancellationRequested();
                        string line = reader.ReadLine();
                        yield return new FileInfo(line.Trim());
                    }
                }
            }
        }

    }
}
