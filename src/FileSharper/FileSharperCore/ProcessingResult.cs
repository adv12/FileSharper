using System.IO;

namespace FileSharperCore
{
    public class ProcessingResult
    {

        public ProcessingResultType Type
        {
            get;
            private set;
        }

        public FileInfo[] OutputFiles
        {
            get;
            private set;
        }

        public ProcessingResult(ProcessingResultType type, FileInfo[] outputFiles)
        {
            Type = type;
            OutputFiles = outputFiles;
        }
    }
}
