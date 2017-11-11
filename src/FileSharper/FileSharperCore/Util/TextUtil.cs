// Copyright (c) 2017 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System;
using System.IO;
using System.Text.RegularExpressions;

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

        public static int GetWordCount(StreamReader reader)
        {
            int wordCount = 0;
            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();
                string[] words = Regex.Split(line, @"\s+");
                for (int i = 0; i < words.Length; i++)
                {
                    if (words[i] != null && words[i].Length > 0)
                    {
                        wordCount++;
                    }
                }
            }
            return wordCount;
        }
    }
}
