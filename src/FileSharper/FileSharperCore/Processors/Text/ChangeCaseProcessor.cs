// Copyright (c) 2017 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace FileSharperCore.Processors.Text
{

    public class ChangeCaseParameters
    {
        [PropertyOrder(1, UsageContextEnum.Both)]
        public TextCase Case { get; set; }
        [PropertyOrder(2, UsageContextEnum.Both)]
        public LineEndings LineEndings { get; set; } = LineEndings.SystemDefault;
        [PropertyOrder(3, UsageContextEnum.Both)]
        public bool MoveOriginalToRecycleBin { get; set; } = true;
    }

    public class ChangeCaseProcessor : LineProcessor
    {
        private ChangeCaseParameters m_Parameters = new ChangeCaseParameters();

        public override string Name => "Convert case";

        public override string Category => "Text";

        public override string Description => "Changes the text to upper- or lowercase";

        public override object Parameters => m_Parameters;

        protected override bool MoveOriginalToRecycleBin => m_Parameters.MoveOriginalToRecycleBin;

        protected override LineEndings LineEndings => m_Parameters.LineEndings;

        protected override string TransformLine(string line)
        {
            if (m_Parameters.Case == TextCase.Uppercase)
            {
                return line.ToUpper();
            }
            else
            {
                return line.ToLower();
            }
        }
    }
}
