// Copyright (c) 2017 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System;
using System.Collections.Generic;
using System.Text;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace FileSharperCore.Processors.Text
{
    public class SpacesToTabsParameters
    {
        [PropertyOrder(1, UsageContextEnum.Both)]
        public int SpacesPerTab { get; set; } = 4;
        [PropertyOrder(2, UsageContextEnum.Both)]
        public bool LeadingSpacesOnly { get; set; }
        [PropertyOrder(3, UsageContextEnum.Both)]
        public LineEndings LineEndings { get; set; } = LineEndings.MatchInput;
        [PropertyOrder(4, UsageContextEnum.Both)]
        public OutputEncodingType OutputEncoding { get; set; } = OutputEncodingType.MatchInput;
        [PropertyOrder(5, UsageContextEnum.Both)]
        public string FileName { get; set; } = ProcessorBase.ORIGINAL_FILE_PATH;
        [PropertyOrder(6, UsageContextEnum.Both)]
        public bool OverwriteExistingFile { get; set; } = true;
        [PropertyOrder(7, UsageContextEnum.Both)]
        public bool MoveOriginalToRecycleBin { get; set; }
    }

    public class SpacesToTabsProcessor : LineProcessor
    {
        private SpacesToTabsParameters m_Parameters = new SpacesToTabsParameters();

        private string m_Spaces;

        public override string Name => "Spaces to tabs";

        public override string Category => "Text";

        public override string Description => "Converts each tab (or each leading tab) to the specified number of spaces";

        public override object Parameters => m_Parameters;

        protected internal override LineEndings LineEndings => m_Parameters.LineEndings;

        protected internal override OutputEncodingType OutputEncodingType => m_Parameters.OutputEncoding;

        protected internal override string FileName => m_Parameters.FileName;

        protected internal override bool OverwriteExistingFile => m_Parameters.OverwriteExistingFile;

        protected internal override bool MoveOriginalToRecycleBin => m_Parameters.MoveOriginalToRecycleBin;

        public override void LocalInit()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < m_Parameters.SpacesPerTab; i++)
            {
                sb.Append(" ");
            }
            m_Spaces = sb.ToString();
        }

        protected internal override string TransformLine(string line)
        {
            if (m_Parameters.LeadingSpacesOnly)
            {
                StringBuilder sb = new StringBuilder();
                int spaceCount = 0;
                for (int i = 0; i < line.Length; i++)
                {
                    if (line[i] == ' ')
                    {
                        spaceCount++;
                        if (spaceCount == m_Parameters.SpacesPerTab)
                        {
                            sb.Append("\t");
                            spaceCount = 0;
                        }
                    }
                    else
                    {
                        sb.Append(line.Substring(i - spaceCount));
                        break;
                    }
                }
                line = sb.ToString();
            }
            else
            {
                line = line.Replace(m_Spaces, "\t");
            }
            return line;
        }
    }
}
