// Copyright (c) 2018 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using FileSharperCore.Util;

namespace FileSharperCore.Conditions.Text
{

    public class TextEncodingTestParameters
    {
        public DetectedEncodingType Encoding { get; set; } = DetectedEncodingType.ASCII;
    }

    public class TextEncodingCondition : ConditionBase
    {
        private TextEncodingTestParameters m_Parameters = new TextEncodingTestParameters();

        public override int ColumnCount => 1;

        public override string[] ColumnHeaders => new string[] { "Encoding" };

        public override string Category => "Text";

        public override string Name => "Text Encoding Matches";

        public override string Description => null;

        public override object Parameters => m_Parameters;

        public override MatchResult Matches(FileInfo file, Dictionary<Type, IFileCache> fileCaches, CancellationToken token)
        {
            MatchResultType resultType = MatchResultType.NotApplicable;
            string encodingString = "N/A";
            try
            {
                DetectedEncodingType encoding = TextUtil.GetDetectedEncodingType(file);
                if (encoding == m_Parameters.Encoding)
                {
                    resultType = MatchResultType.Yes;
                }
                else
                {
                    resultType = MatchResultType.No;
                }
                encodingString = encoding.ToString();
            }
            catch (Exception)
            {
                
            }
            return new MatchResult(resultType, encodingString);
        }
    }
}
