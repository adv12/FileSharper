// Copyright (c) 2018 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System;
using System.IO;
using System.Threading;
using FileSharperCore.Util;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace FileSharperCore.Processors.Filesystem
{

    public class CreateOrUpdateFileParameters
    {
        [PropertyOrder(1, UsageContextEnum.Both)]
        public string FileName { get; set; } = @"{DirectoryName}\{NameWithoutExtension}_emptyFile.txt";
        [PropertyOrder(2, UsageContextEnum.Both)]
        public bool UpdateModificationDateIfExists { get; set; } = true;
    }

    public class CreateOrUpdateFileProcessor : SingleFileProcessorBase
    {
        private CreateOrUpdateFileParameters m_Parameters = new CreateOrUpdateFileParameters();

        public override string Category => "Filesystem";

        public override string Name => "Create or update file";

        public override string Description => @"Creates a file if it doesn't exist, optionally updating its modification date if it does exist (similar to UNIX ""touch"")";

        public override object Parameters => m_Parameters;

        protected internal override ProcessingResult Process(FileInfo file, string[] values, CancellationToken token)
        {
            ProcessingResultType resultType = ProcessingResultType.Success;
            string message = "Success";
            string newPath = ReplaceUtil.Replace(m_Parameters.FileName, file);
            FileInfo[] resultFiles = new FileInfo[0];
            if (File.Exists(newPath))
            {
                if (m_Parameters.UpdateModificationDateIfExists)
                {
                    try
                    {
                        File.SetLastWriteTime(newPath, DateTime.Now);
                        resultFiles = new FileInfo[] { new FileInfo(newPath) };
                        message = "Successfully updated file modification date";
                    }
                    catch (Exception ex)
                    {
                        resultType = ProcessingResultType.Failure;
                        message = $"Failed to update file modification date: {ex.Message}";
                        RunInfo.ExceptionInfos.Enqueue(new ExceptionInfo(ex, file));
                    }
                }
            }
            else
            {
                try
                {
                    File.Create(newPath).Close();
                    resultFiles = new FileInfo[] { new FileInfo(newPath) };
                    message = "Successfully created file";
                }
                catch (Exception ex)
                {
                    resultType = ProcessingResultType.Failure;
                    message = $"Failed to create file: {ex.Message}";
                    RunInfo.ExceptionInfos.Enqueue(new ExceptionInfo(ex, file));
                }
            }
            return new ProcessingResult(resultType, message, resultFiles);
        }
    }
}
