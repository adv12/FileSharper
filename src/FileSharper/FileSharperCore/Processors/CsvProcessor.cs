// Copyright (c) 2017 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System;
using System.Collections.Generic;
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
        public PathFormat PathFormat { get; set; }
        [PropertyOrder(3, UsageContextEnum.Both)]
        public LineEndingsNoFile LineEndings { get; set; } = LineEndingsNoFile.SystemDefault;
        [PropertyOrder(4, UsageContextEnum.Both)]
        public bool AutoOpen { get; set; }
    }

    public class CsvProcessor : ProcessorBase
    {
        private CsvParameters m_Parameters = new CsvParameters();

        private CsvWriter m_CsvWriter;

        public override string Name => "Write to CSV";

        public override string Category => "\u0002\u0002Report";

        public override string Description => "Writes the results to the specified CSV file";

        public override object Parameters => m_Parameters;

        private string m_Filename;

        public override void LocalInit(IList<ExceptionInfo> exceptionInfos)
        {
            base.LocalInit(exceptionInfos);
            m_Filename = ReplaceUtil.Replace(m_Parameters.Filename, (FileInfo)null);
            TextWriter tw = new StreamWriter(m_Filename);
            string lineEnding = TextUtil.GetNewline(m_Parameters.LineEndings);
            m_CsvWriter = new CsvWriter(tw);
            List<string> headers = new List<string>();
            switch (m_Parameters.PathFormat)
            {
                case PathFormat.FullPath:
                    headers.Add("Filename");
                    break;
                case PathFormat.NameThenDirectory:
                    headers.Add("Filename");
                    headers.Add("Path");
                    break;
                case PathFormat.DirectoryThenName:
                    headers.Add("Path");
                    headers.Add("Filename");
                    break;
            }
            headers.Add("Matches");
            string[] columnHeaders = HeaderUtil.GetUniqueHeaders(headers, RunInfo.Condition, RunInfo.FieldSources);
            foreach (string header in columnHeaders)
            {
                m_CsvWriter.WriteField(header);
            }
            m_CsvWriter.NextRecord();
        }

        public override ProcessingResult Process(FileInfo originalFile,
            MatchResultType matchResultType, string[] values,
            FileInfo[] generatedFiles, ProcessInput whatToProcess,
            IList<ExceptionInfo> exceptionInfos, CancellationToken token)
        {
            switch (m_Parameters.PathFormat)
            {
                case PathFormat.FullPath:
                    m_CsvWriter.WriteField(originalFile.FullName);
                    break;
                case PathFormat.NameThenDirectory:
                    m_CsvWriter.WriteField(originalFile.Name);
                    m_CsvWriter.WriteField(originalFile.DirectoryName);
                    break;
                case PathFormat.DirectoryThenName:
                    m_CsvWriter.WriteField(originalFile.DirectoryName);
                    m_CsvWriter.WriteField(originalFile.Name);
                    break;
            }
            m_CsvWriter.WriteField(matchResultType.ToString());
            foreach (string value in values)
            {
                m_CsvWriter.WriteField(value);
            }
            m_CsvWriter.NextRecord();
            return new ProcessingResult(ProcessingResultType.Success, "Success", new FileInfo[] { originalFile });
        }

        public override void LocalCleanup(IList<ExceptionInfo> exceptionInfos)
        {
            m_CsvWriter?.Dispose();
            base.LocalCleanup(exceptionInfos);
            if (m_Parameters.AutoOpen)
            {
                try
                {
                    System.Diagnostics.Process.Start(m_Filename);
                }
                catch (Exception)
                {

                }
            }
        }
    }
}
