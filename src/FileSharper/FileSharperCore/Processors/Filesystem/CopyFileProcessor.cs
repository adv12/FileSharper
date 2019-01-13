// Copyright (c) 2017 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System;
using System.IO;
using System.Threading;
using FileSharperCore.Util;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace FileSharperCore.Processors.Filesystem
{
    public class CopyFileParameters
    {
        [PropertyOrder(1, UsageContextEnum.Both)]
        public string NewPath { get; set; } = @"{Desktop}\{Name}";
        [PropertyOrder(2, UsageContextEnum.Both)]
        public bool Overwrite { get; set; } = false;
    }

    public class CopyFileProcessor : SingleFileProcessorBase
    {
        private CopyFileParameters m_Parameters = new CopyFileParameters();

        public override string Name => "Copy file";

        public override string Category => "Filesystem";

        public override string Description => "Copy file to the specified location";

        public override object Parameters => m_Parameters;

        public override ProcessingResult Process(FileInfo file, string[] values,
            CancellationToken token)
        {
            string newPath = ReplaceUtil.Replace(m_Parameters.NewPath, file);
            string message = "Success";
            FileInfo result = null;
            try
            {
                string dirPath = Path.GetDirectoryName(newPath);
                Directory.CreateDirectory(dirPath);
                if (m_Parameters.Overwrite || !File.Exists(newPath))
                {
                    result = file.CopyTo(newPath, m_Parameters.Overwrite);
                }
            }
            catch (Exception ex)
            {
                RunInfo.ExceptionInfos.Enqueue(new ExceptionInfo(ex, file));
                message = ex.Message;
            }
            if (result == null)
            {
                return new ProcessingResult(ProcessingResultType.Failure, message ?? "Failure", new FileInfo[0]);
            }
            return new ProcessingResult(ProcessingResultType.Success, "Success", new FileInfo[] { new FileInfo(newPath) });
        }
    }
}
