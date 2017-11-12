// Copyright (c) 2017 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using FileSharperCore.Util;

namespace FileSharperCore.Conditions.Text
{

    public class LineEndingsParameters
    {
        public DetectedLineEndings LineEndings { get; set; }
    }

    public class LineEndingsCondition : ConditionBase
    {

        private LineEndingsParameters m_Parameters = new LineEndingsParameters();

        public override int ColumnCount => 1;

        public override string[] ColumnHeaders => new string[] { "Line Endings" };

        public override string Name => "Line Endings";

        public override string Category => "Text";

        public override string Description => "Tests whether the line endings match the specified value";

        public override object Parameters => m_Parameters;

        public override MatchResult Matches(FileInfo file, Dictionary<Type, IFileCache> fileCaches, CancellationToken token)
        {
            MatchResultType resultType = MatchResultType.NotApplicable;
            DetectedLineEndings lineEndings = DetectedLineEndings.NotApplicable;
            try
            {
                using (StreamReader reader = new StreamReader(file.FullName))
                {
                    lineEndings = TextUtil.GetLineEndings(reader, token);
                }
                if (lineEndings == m_Parameters.LineEndings)
                {
                    resultType = MatchResultType.Yes;
                }
                else
                {
                    resultType = MatchResultType.No;
                }
            }
            catch (Exception ex)
            {

            }
            return new MatchResult(resultType, lineEndings.ToString());
        }
    }
}
