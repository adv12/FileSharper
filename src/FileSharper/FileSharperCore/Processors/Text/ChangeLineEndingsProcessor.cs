// Copyright (c) 2017 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace FileSharperCore.Processors.Text
{
    public class ChangeLineEndingsParameters
    {
        [PropertyOrder(1, UsageContextEnum.Both)]
        public LineEndings LineEndings { get; set; } = LineEndings.SystemDefault;
        [PropertyOrder(2, UsageContextEnum.Both)]
        public OutputEncoding OutputEncoding { get; set; } = OutputEncoding.MatchInput;
        [PropertyOrder(3, UsageContextEnum.Both)]
        public string FileName { get; set; } = ProcessorBase.ORIGINAL_FILE_PATH;
        [PropertyOrder(4, UsageContextEnum.Both)]
        public bool OverwriteExistingFile { get; set; } = true;
        [PropertyOrder(5, UsageContextEnum.Both)]
        public bool MoveOriginalToRecycleBin { get; set; }
    }

    public class ChangeLineEndingsProcessor : LineProcessor
    {
        private ChangeLineEndingsParameters m_Parameters = new ChangeLineEndingsParameters();

        public override string Name => "Change line endings";

        public override string Category => "Text";

        public override string Description => "Changes the line endings to the selected type";

        public override object Parameters => m_Parameters;

        protected override bool MoveOriginalToRecycleBin => m_Parameters.MoveOriginalToRecycleBin;

        protected override LineEndings LineEndings => m_Parameters.LineEndings;

        protected override OutputEncoding OutputEncoding => m_Parameters.OutputEncoding;

        protected override string FileName => m_Parameters.FileName;

        protected override bool OverwriteExistingFile => m_Parameters.OverwriteExistingFile;

        protected override string TransformLine(string line)
        {
            return line;
        }
    }
}
