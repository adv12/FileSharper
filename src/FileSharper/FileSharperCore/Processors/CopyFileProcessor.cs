// Copyright (c) 2017 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System;
using System.IO;
using System.Threading;
using FileSharperCore.Util;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace FileSharperCore.Processors
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

        public override string Description => "Copy file to the specified location";

        public override object Parameters => m_Parameters;

        public override HowOften ProducesFiles => HowOften.Always;

        public override ProcessingResult Process(FileInfo file, string[] values,
            IProgress<ExceptionInfo> exceptionProgress, CancellationToken token)
        {
            string newPath = ReplaceUtil.Replace(m_Parameters.NewPath, file);
            try
            {
                string dirPath = Path.GetDirectoryName(newPath);
                Directory.CreateDirectory(dirPath);
                file.CopyTo(newPath, m_Parameters.Overwrite);
            }
            catch (Exception ex)
            {
                exceptionProgress.Report(new ExceptionInfo(ex, file));
                return new ProcessingResult(ProcessingResultType.Failure, ex.Message, new FileInfo[0]);
            }
            return new ProcessingResult(ProcessingResultType.Success, "Success", new FileInfo[] { new FileInfo(newPath) });
        }
    }
}
