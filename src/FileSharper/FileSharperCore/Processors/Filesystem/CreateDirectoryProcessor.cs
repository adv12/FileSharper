// Copyright (c) 2019 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System;
using System.IO;
using System.Threading;
using FileSharperCore.Util;

namespace FileSharperCore.Processors.Filesystem
{
    public class CreateDirectoryParameters
    {
        public string DirectoryPath { get; set; } = @"{DirectoryName}\NewSubdirectory";
    }

    public class CreateDirectoryProcessor : SingleFileProcessorBase
    {
        private CreateDirectoryParameters m_Parameters = new CreateDirectoryParameters();

        public override string Category => "Filesystem";

        public override string Name => "Create directory";

        public override string Description => "Create a directory if it does not exist";

        public override object Parameters => m_Parameters;

        protected internal override ProcessingResult Process(FileInfo file, string[] values, CancellationToken token)
        {
            string directoryPath = ReplaceUtil.Replace(m_Parameters.DirectoryPath, file);
            ProcessingResultType resultType = ProcessingResultType.Failure;
            string message = "Failed to create directory";
            try
            {
                if (Directory.Exists(directoryPath))
                {
                    resultType = ProcessingResultType.Success;
                    message = "Directory already exists";
                }
                else if (!File.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                    resultType = ProcessingResultType.Success;
                    message = "Directory created";
                }
            }
            catch (Exception ex)
            {
                RunInfo.ExceptionInfos.Enqueue(new ExceptionInfo(ex, file));
                message = ex.Message;
            }
            return new ProcessingResult(resultType, message, new FileInfo[0]);
        }
    }
}
