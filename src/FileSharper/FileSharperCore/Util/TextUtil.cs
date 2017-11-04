using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
