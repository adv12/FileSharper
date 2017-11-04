// Copyright (c) 2017 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System;
using System.ComponentModel;
using System.IO;
using System.Threading;
using CsvHelper;
using FileSharperCore.Editors;
using FileSharperCore.Util;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace FileSharperCore.Processors
{
    public class CsvParameters
    {
        [PropertyOrder(1, UsageContextEnum.Both)]
        [Editor(typeof(SaveFileEditor), typeof(SaveFileEditor))]
        public string Filename { get; set; }
        [PropertyOrder(2, UsageContextEnum.Both)]
        public LineEndings LineEndings { get; set; }
    }

    public class CsvProcessor : ProcessorBase
    {
        private CsvParameters m_Parameters = new CsvParameters();

        private CsvWriter m_CsvWriter;

        public override string Name => "Write to CSV";

        public override string Description => "Writes the results to the specified CSV file";

        public override object Parameters => m_Parameters;

        public override void LocalInit(IProgress<ExceptionInfo> exceptionProgress)
        {
            base.LocalInit(exceptionProgress);
            string filename = ReplaceUtil.Replace(m_Parameters.Filename, (FileInfo)null);
            TextWriter tw = new StreamWriter(filename);
            string lineEnding = TextUtil.GetNewline(m_Parameters.LineEndings);
            m_CsvWriter = new CsvWriter(tw);
            m_CsvWriter.WriteField("Filename");
            m_CsvWriter.WriteField("Path");
            foreach (string header in this.RunInfo.Condition.ColumnHeaders)
            {
                m_CsvWriter.WriteField(header);
            }
            foreach (IOutput output in this.RunInfo.Outputs)
            {
                foreach (string header in output.ColumnHeaders)
                {
                    m_CsvWriter.WriteField(header);
                }
            }
            m_CsvWriter.NextRecord();
        }

        public override ProcessingResult Process(FileInfo originalFile, string[] values,
            FileInfo[] generatedFiles, ProcessInput whatToProcess,
            IProgress<ExceptionInfo> exceptionProgress, CancellationToken token)
        {
            m_CsvWriter.WriteField(originalFile.Name);
            m_CsvWriter.WriteField(originalFile.DirectoryName);
            foreach (string value in values)
            {
                m_CsvWriter.WriteField(value);
            }
            m_CsvWriter.NextRecord();
            return new ProcessingResult(ProcessingResultType.Success, "Success", new FileInfo[] { originalFile });
        }

        public override void LocalCleanup(IProgress<ExceptionInfo> exceptionProgress)
        {
            m_CsvWriter?.Dispose();
            base.LocalCleanup(exceptionProgress);
        }
    }
}
