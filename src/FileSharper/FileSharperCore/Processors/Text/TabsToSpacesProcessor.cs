// Copyright (c) 2017 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System;
using System.Text;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace FileSharperCore.Processors.Text
{
    public class TabsToSpacesParameters
    {
        [PropertyOrder(1, UsageContextEnum.Both)]
        public int SpacesPerTab { get; set; } = 4;
        [PropertyOrder(2, UsageContextEnum.Both)]
        public bool LeadingTabsOnly { get; set; }
        [PropertyOrder(3, UsageContextEnum.Both)]
        public LineEndings LineEndings { get; set; } = LineEndings.SystemDefault;
        [PropertyOrder(4, UsageContextEnum.Both)]
        public bool MoveOriginalToRecycleBin { get; set; } = true;
    }

    public class TabsToSpacesProcessor : LineProcessor
    {
        private TabsToSpacesParameters m_Parameters = new TabsToSpacesParameters();

        private string m_Spaces;

        public override string Name => "Tabs to spaces";

        public override string Category => "Text";

        public override string Description => "Converts each tab (or each leading tab) to the specified number of spaces";

        public override object Parameters => m_Parameters;

        protected override LineEndings LineEndings => m_Parameters.LineEndings;

        protected override bool MoveOriginalToRecycleBin => m_Parameters.MoveOriginalToRecycleBin;

        public override void LocalInit(IProgress<ExceptionInfo> exceptionProgress)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < m_Parameters.SpacesPerTab; i++)
            {
                sb.Append(" ");
            }
            m_Spaces = sb.ToString();
        }

        protected override string TransformLine(string line)
        {
            if (m_Parameters.LeadingTabsOnly)
            {
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < line.Length; i++)
                {
                    if (line[i] == '\t')
                    {
                        sb.Append(m_Spaces);
                    }
                    else
                    {
                        sb.Append(line.Substring(i));
                        break;
                    }
                }
                line = sb.ToString();
            }
            else
            {
                line = line.Replace("\t", m_Spaces);
            }
            return line;
        }
    }
}
