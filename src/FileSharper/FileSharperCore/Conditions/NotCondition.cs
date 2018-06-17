// Copyright (c) 2017 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace FileSharperCore.Conditions
{
    public class NotCondition : ConditionBase
    {
        public ICondition Condition
        {
            get;
            set;
        }

        public override object Parameters { get; } = new object();

        public override string Name => "Not";

        public override string Category => "Miscellaneous";

        public override string Description => "Returns the opposite of the provided condition";

        public override int ColumnCount
        {
            get
            {
                if (Condition == null)
                {
                    return 0;
                }
                return Condition.ColumnCount;
            }
        }

        public override string[] ColumnHeaders
        {
            get
            {
                if (Condition == null)
                {
                    return new string[0];
                }
                return Condition.ColumnHeaders;
            }
        }

        public override Type[] CacheTypes => Condition.CacheTypes;

        public NotCondition()
        {

        }

        public NotCondition(ICondition condition)
        {
            Condition = condition;
        }

        public override void LocalInit(IList<ExceptionInfo> exceptionInfos)
        {
            Condition?.Init(RunInfo, exceptionInfos);
        }

        public override void LocalCleanup(IList<ExceptionInfo> exceptionInfos)
        {
            Condition?.Cleanup(exceptionInfos);
        }

        public override MatchResult Matches(FileInfo file, Dictionary<Type, IFileCache> fileCaches, CancellationToken token)
        {
            MatchResult r = Condition.Matches(file, fileCaches, token);
            MatchResultType type = MatchResultType.NotApplicable;
            if (r.Type == MatchResultType.Yes)
            {
                type = MatchResultType.No;
            }
            else if (r.Type == MatchResultType.No)
            {
                type = MatchResultType.Yes;
            }
            return new MatchResult(type, r.Values);
        }
    }
}
