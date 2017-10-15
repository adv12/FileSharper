// Copyright (c) 2017 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System;
using System.IO;

namespace FileSharperCore
{
    public class ExceptionInfo
    {
        public Exception Exception { get; set; }
        public FileInfo File { get; set; }

        public ExceptionInfo(Exception exception, FileInfo file = null)
        {
            Exception = exception;
            File = file;
        }
    }
}
