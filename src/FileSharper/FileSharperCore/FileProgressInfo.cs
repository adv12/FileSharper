// Copyright (c) 2017 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System.IO;

namespace FileSharperCore
{
    public class FileProgressInfo
    {
        public FileInfo File;
        public MatchResultType ResultType;
        public string[] Values;
        public FileProgressInfo(FileInfo file, MatchResultType resultType, string[] values)
        {
            File = file;
            ResultType = resultType;
            Values = values;
        }
    }
}
