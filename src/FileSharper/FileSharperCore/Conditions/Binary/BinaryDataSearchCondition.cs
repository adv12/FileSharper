// Copyright (c) 2018 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace FileSharperCore.Conditions.Binary
{
    public abstract class BinaryDataSearchCondition : ConditionBase
    {
        protected abstract byte[] Bytes { get; }

        public override MatchResult Matches(FileInfo file, Dictionary<Type, IFileCache> fileCaches, CancellationToken token)
        {
            LinkedList<byte> currentBytes = new LinkedList<byte>();
            byte[] bytes = this.Bytes;
            using (FileStream stream = file.OpenRead())
            {
                int b;
                if (stream.Length < bytes.Length)
                {
                    return new MatchResult(MatchResultType.No, "No");
                }
                for (int i = 0; i < bytes.Length - 1; i++)
                {
                    currentBytes.AddLast((byte)stream.ReadByte());
                }
                while ((b = stream.ReadByte()) != -1)
                {
                    currentBytes.AddLast((byte)b);
                    bool match = true;
                    int i = 0;
                    foreach (byte cb in currentBytes)
                    {
                        if (bytes[i++] != cb)
                        {
                            match = false;
                            break;
                        }
                    }
                    if (match)
                    {
                        return new MatchResult(MatchResultType.Yes, "Yes");
                    }
                    currentBytes.RemoveFirst();
                }
            }
            return new MatchResult(MatchResultType.No, "No");
        }

    }


}
