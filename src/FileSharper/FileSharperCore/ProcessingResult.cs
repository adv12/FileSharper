// Copyright (c) 2017 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

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

        public string Message
        {
            get;
            private set;
        }

        public FileInfo[] OutputFiles
        {
            get;
            private set;
        }

        public ProcessingResult(ProcessingResultType type, string message, FileInfo[] outputFiles)
        {
            Type = type;
            Message = message;
            OutputFiles = outputFiles;
        }
    }
}
