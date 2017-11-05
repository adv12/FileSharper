// Copyright (c) 2017 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System;

namespace FileSharperCore.Util
{
    public class TextUtil
    {
        public static string GetNewline(LineEndings lineEndings)
        {
            switch (lineEndings)
            {
                case LineEndings.Windows:
                    return "\r\n";
                case LineEndings.Unix:
                    return "\n";
                case LineEndings.OldMacOS:
                    return "\r";
                default:
                    return Environment.NewLine;
            }
        }
    }
}
