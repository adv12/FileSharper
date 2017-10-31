// Copyright (c) 2017 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace FileSharperCore.Processors.Shell
{
    public class OpenContainingFolderProcessor : SingleFileProcessorBase
    {
        private HashSet<string> m_OpenedFolders = new HashSet<string>();

        public override string Name => "Open containing folder";

        public override string Category => "Shell";

        public override string Description => "Opens the folder containing the file";

        public override object Parameters => null;

        public override void LocalInit(IProgress<ExceptionInfo> exceptionProgress)
        {
            m_OpenedFolders.Clear();
        }

        public override ProcessingResult Process(FileInfo file, string[] values,
            IProgress<ExceptionInfo> exceptionProgress, CancellationToken token)
        {
            if (!m_OpenedFolders.Contains(file.DirectoryName))
            {
                m_OpenedFolders.Add(file.DirectoryName);
                System.Diagnostics.Process.Start(file.DirectoryName);
            }
            return new ProcessingResult(ProcessingResultType.Success, "Success", null);
        }
    }
}
