// Copyright (c) 2017 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System;
using System.Collections.Generic;
using System.Linq;

namespace FileSharperCore.Conditions
{
    public abstract class CompoundCondition: ConditionBase
    {
        public List<ICondition> Conditions { get; } = new List<ICondition>();

        public override int ColumnCount
        {
            get
            {
                int count = 0;
                foreach (ICondition c in Conditions)
                {
                    count += c.ColumnCount;
                }
                return count;
            }
        }

        public override string Category => "\u0002Compound";

        public override string[] ColumnHeaders
        {
            get
            {
                List<string> headers = new List<string>();
                foreach (ICondition c in Conditions)
                {
                    headers.AddRange(c.ColumnHeaders);
                }
                return headers.ToArray();
            }
        }

        public override Type[] CacheTypes
        {
            get
            {
                HashSet<Type> types = new HashSet<Type>();
                foreach (ICondition condition in Conditions)
                {
                    foreach (Type t in condition.CacheTypes)
                    {
                        types.Add(t);
                    }
                }
                return types.ToArray();
            }
        }

        public override void LocalInit()
        {
            foreach (ICondition condition in Conditions)
            {
                try
                {
                    condition.Init(RunInfo);
                }
                catch (Exception ex)
                {
                    RunInfo.ExceptionInfos.Enqueue(new ExceptionInfo(ex));
                }
            }
        }

        public override void LocalCleanup()
        {
            foreach (ICondition condition in Conditions)
            {
                try
                {
                    condition.Cleanup();
                }
                catch (Exception ex)
                {
                    RunInfo.ExceptionInfos.Enqueue(new ExceptionInfo(ex));
                }
            }
        }

    }
}
