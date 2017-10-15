// Copyright (c) 2017 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

namespace FileSharperCore
{
    public class RunInfo
    {
        public IFileSource FileSource
        {
            get;
            private set;
        }

        public ICondition Condition
        {
            get;
            private set;
        }

        public IOutput[] Outputs
        {
            get;
            private set;
        }

        public IProcessor[] TestedProcessors
        {
            get;
            private set;
        }

        public IProcessor[] MatchedProcessors
        {
            get;
            private set;
        }

        public RunInfo(IFileSource fileSource, ICondition condition,
            IOutput[] outputs, IProcessor[] testedProcessors, IProcessor[] matchedProcessors)
        {
            FileSource = fileSource;
            Condition = condition;
            Outputs = outputs;
            TestedProcessors = testedProcessors;
            MatchedProcessors = matchedProcessors;
        }
    }
}
