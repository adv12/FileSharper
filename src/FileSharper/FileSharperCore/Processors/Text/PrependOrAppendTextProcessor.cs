// Copyright (c) 2017 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using FileSharperCore.Util;
using Microsoft.VisualBasic.FileIO;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace FileSharperCore.Processors
{
    public class TextParameter
    {
        [PropertyOrder(1, UsageContextEnum.Both)]
        public PrependAppend PrependOrAppend { get; set; }
        [PropertyOrder(2, UsageContextEnum.Both)]
        public List<string> Text { get; set; } = new List<string>();
        [PropertyOrder(3, UsageContextEnum.Both)]
        public LineEndings LineEndings { get; set; }
        [PropertyOrder(4, UsageContextEnum.Both)]
        public bool MoveOriginalToRecycleBin { get; set; } = true;
    }

    public class PrependOrAppendTextProcessor : SingleFileProcessorBase
    {
        private TextParameter m_Parameters = new TextParameter();

        public override string Name => "Prepend or append text";

        public override string Category => "Text";

        public override string Description => "Prepend or append the specified text to the file";

        public override object Parameters => m_Parameters;

        public override ProcessingResult Process(FileInfo file, string[] values,
            IProgress<ExceptionInfo> exceptionProgress, CancellationToken token)
        {
            string tmpFile = Path.GetTempFileName();
            using (StreamWriter writer = new StreamWriter(tmpFile))
            {
                writer.NewLine = TextUtil.GetNewline(m_Parameters.LineEndings);
                using (StreamReader reader = new StreamReader(file.FullName))
                {
                    if (m_Parameters.PrependOrAppend == PrependAppend.Prepend)
                    {
                        foreach (string line in m_Parameters.Text)
                        {
                            writer.WriteLine(line);
                        }
                    }
                    while (!reader.EndOfStream)
                    {
                        writer.WriteLine(reader.ReadLine());
                    }
                    if (m_Parameters.PrependOrAppend == PrependAppend.Append)
                    {
                        foreach (string line in m_Parameters.Text)
                        {
                            writer.WriteLine(line);
                        }
                    }
                }
            }
            if (m_Parameters.MoveOriginalToRecycleBin)
            {
                FileSystem.DeleteFile(file.FullName, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin,
                UICancelOption.DoNothing);
            }
            File.Copy(tmpFile, file.FullName, true);
            File.Delete(tmpFile);
            return new ProcessingResult(ProcessingResultType.Success, "Success", null);
        }
    }
}
