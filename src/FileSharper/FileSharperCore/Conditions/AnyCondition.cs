// Copyright (c) 2017 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace FileSharperCore.Conditions
{
    public class AnyCondition : CompoundCondition
    {

        public override object Parameters { get; } = new object();

        public override string Name => "Any";

        public override string Description => "Returns Yes if any of the specified conditions returns Yes";

        public override MatchResult Matches(FileInfo file, Dictionary<Type, IFileCache> fileCaches, CancellationToken token)
        {
            List<string> values = new List<string>();
            MatchResultType type = MatchResultType.No;
            foreach (ICondition c in Conditions)
            {
                token.ThrowIfCancellationRequested();
                MatchResult result = c.Matches(file, fileCaches, token);
                if (result.Values != null)
                {
                    values.AddRange(result.Values);
                }
                if (result.Type == MatchResultType.Yes)
                {
                    type = MatchResultType.Yes;
                    // DO NOT short-circuit.  We need all conditions to be evaluated for their ouptut values.
                }
            }
            return new MatchResult(type, values.ToArray());
        }
    }
}
