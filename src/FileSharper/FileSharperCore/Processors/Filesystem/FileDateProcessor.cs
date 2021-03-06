﻿// Copyright (c) 2018 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System;
using System.IO;
using System.Threading;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace FileSharperCore.Processors.Filesystem
{
    public class FileDateProcessorParameters
    {
        [PropertyOrder(1, UsageContextEnum.Both)]
        public FileDateType FileDateType { get; set; } = FileDateType.Modified;
        [PropertyOrder(2, UsageContextEnum.Both)]
        public DateTime Date { get; set; } = DateTime.Now;
    }

    public class FileDateProcessor : SingleFileProcessorBase
    {
        private FileDateProcessorParameters m_Parameters = new FileDateProcessorParameters();

        public override string Name => "Set file date";

        public override string Category => "Filesystem";

        public override string Description => "Sets the specified file date property to the specified date";

        public override object Parameters => m_Parameters;

        protected internal override ProcessingResult Process(FileInfo file, string[] values,
            CancellationToken token)
        {
            try
            {
                switch (m_Parameters.FileDateType)
                {
                    case FileDateType.Accessed:
                        File.SetLastAccessTime(file.FullName, m_Parameters.Date);
                        break;
                    case FileDateType.Created:
                        File.SetCreationTime(file.FullName, m_Parameters.Date);
                        break;
                    case FileDateType.Modified:
                        File.SetLastWriteTime(file.FullName, m_Parameters.Date);
                        break;
                }
            }
            catch (Exception ex)
            {
                RunInfo.ExceptionInfos.Enqueue(new ExceptionInfo(ex, file));
                return new ProcessingResult(ProcessingResultType.Failure, ex.Message, new FileInfo[0]);
            }
            return new ProcessingResult(ProcessingResultType.Success, "Success", new FileInfo[] { file });
        }
    }
}
