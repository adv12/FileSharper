// Copyright (c) 2017 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace FileSharperCore.Processors.Text
{
    public class TrimWhitespaceParameters
    {
        [PropertyOrder(1, UsageContextEnum.Both)]
        public TrimType Where { get; set; }
        [PropertyOrder(2, UsageContextEnum.Both)]
        public LineEndings LineEndings { get; set; } = LineEndings.SystemDefault;
        [PropertyOrder(3, UsageContextEnum.Both)]
        public bool MoveOriginalToRecycleBin { get; set; } = true;
    }

    public class TrimWhitespaceProcessor : LineProcessor
    {
        private TrimWhitespaceParameters m_Parameters = new TrimWhitespaceParameters();

        public override string Name => "Trim whitespace";

        public override string Category => "Text";

        public override string Description => "Trims leading whitespace, trailing whitespace, or both from each line";

        public override object Parameters => m_Parameters;

        protected override LineEndings LineEndings => m_Parameters.LineEndings;

        protected override bool MoveOriginalToRecycleBin => m_Parameters.MoveOriginalToRecycleBin;

        protected override string TransformLine(string line)
        {
            switch (m_Parameters.Where)
            {
                case TrimType.Start:
                    return line.TrimStart();
                case TrimType.End:
                    return line.TrimEnd();
                default:
                    return line.Trim();
            }
        }
    }
}
