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

        public override void LocalInit()
        {
            m_OpenedFolders.Clear();
        }

        protected internal override ProcessingResult Process(FileInfo file, string[] values,
            CancellationToken token)
        {
            ProcessingResultType type = ProcessingResultType.Failure;
            string message = "Success";
            if (!m_OpenedFolders.Contains(file.DirectoryName))
            {
                try
                {
                    System.Diagnostics.Process.Start(file.DirectoryName);
                    m_OpenedFolders.Add(file.DirectoryName);
                    type = ProcessingResultType.Success;
                }
                catch (Exception ex)
                {
                    message = ex.Message;
                    RunInfo.ExceptionInfos.Enqueue(new ExceptionInfo(ex, file));
                }
            }
            return new ProcessingResult(type, message, new FileInfo[] { file });
        }
    }
}
