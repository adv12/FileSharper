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
        public LineEndings LineEndings { get; set; } = LineEndings.MatchInput;
        [PropertyOrder(3, UsageContextEnum.Both)]
        public OutputEncodingType OutputEncoding { get; set; } = OutputEncodingType.MatchInput;
        [PropertyOrder(4, UsageContextEnum.Both)]
        public string FileName { get; set; } = ProcessorBase.ORIGINAL_FILE_PATH;
        [PropertyOrder(5, UsageContextEnum.Both)]
        public bool OverwriteExistingFile { get; set; } = true;
        [PropertyOrder(6, UsageContextEnum.Both)]
        public bool MoveOriginalToRecycleBin { get; set; }
    }

    public class ChangeCaseProcessor : LineProcessor
    {
        private ChangeCaseParameters m_Parameters = new ChangeCaseParameters();

        public override string Name => "Convert case";

        public override string Category => "Text";

        public override string Description => "Changes the text to upper- or lowercase";

        public override object Parameters => m_Parameters;

        protected internal override bool MoveOriginalToRecycleBin => m_Parameters.MoveOriginalToRecycleBin;

        protected internal override LineEndings LineEndings => m_Parameters.LineEndings;

        protected internal override OutputEncodingType OutputEncodingType => m_Parameters.OutputEncoding;

        protected internal override string FileName => m_Parameters.FileName;

        protected internal override bool OverwriteExistingFile => m_Parameters.OverwriteExistingFile;

        protected internal override string TransformLine(string line)
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
