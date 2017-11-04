﻿// Copyright (c) 2017 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using Microsoft.VisualBasic.FileIO;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace FileSharperCore.Processors.Text
{

    public class ReplaceTextParameters
    {
        [PropertyOrder(1, UsageContextEnum.Both)]
        public string TextToMatch { get; set; }
        [PropertyOrder(2, UsageContextEnum.Both)]
        public string ReplacementText { get; set; }
        [PropertyOrder(3, UsageContextEnum.Both)]
        public bool UseRegex { get; set; }
        [PropertyOrder(4, UsageContextEnum.Both)]
        public bool Multiline { get; set; }
        [PropertyOrder(5, UsageContextEnum.Both)]
        public bool CaseSensitive { get; set; }
        [PropertyOrder(6, UsageContextEnum.Both)]
        public bool MoveOriginalToRecycleBin { get; set; } = true;
    }

    public class ReplaceTextProcessor : SingleFileProcessorBase
    {
        private ReplaceTextParameters m_Parameters = new ReplaceTextParameters();

        private Regex m_Regex;

        private string m_ReplacementText;

        public override string Name => "Replace text";

        public override string Category => "Text";

        public override string Description => "Replace text, optionally using a regular expression";

        public override object Parameters => m_Parameters;

        public override void LocalInit(IProgress<ExceptionInfo> exceptionProgress)
        {
            RegexOptions regexOptions = RegexOptions.None;
            if (!m_Parameters.CaseSensitive)
            {
                regexOptions |= RegexOptions.IgnoreCase;
            }
            if (m_Parameters.Multiline)
            {
                regexOptions |= RegexOptions.Multiline;
            }
            if (m_Parameters.UseRegex)
            {
                m_Regex = new Regex(m_Parameters.TextToMatch, regexOptions);
                m_ReplacementText = m_Parameters.ReplacementText;
            }
            else
            {
                m_Regex = new Regex(Regex.Escape(m_Parameters.TextToMatch), regexOptions);
                m_ReplacementText = m_Parameters.ReplacementText.Replace("$", "$$");
            }
        }

        public override ProcessingResult Process(FileInfo file, string[] values, IProgress<ExceptionInfo> exceptionProgress, CancellationToken token)
        {
            string tmpFile = Path.GetTempFileName();
            using (StreamWriter writer = new StreamWriter(tmpFile))
            {
                if (m_Parameters.Multiline)
                {
                    string text = File.ReadAllText(file.FullName);
                    text = m_Regex.Replace(text, m_ReplacementText);
                    writer.Write(text);
                }
                else
                {
                    using (StreamReader reader = new StreamReader(file.FullName))
                    {
                        while (!reader.EndOfStream)
                        {
                            string line = reader.ReadLine();
                            line = m_Regex.Replace(line, m_ReplacementText);
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