// Copyright (c) 2017 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;

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

        public static int GetWordCount(StreamReader reader, CancellationToken token)
        {
            int wordCount = 0;
            while (!reader.EndOfStream)
            {
                token.ThrowIfCancellationRequested();
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

        public static int GetLineCount(StreamReader reader, CancellationToken token)
        {
            int lineCount = 0;
            while (!reader.EndOfStream)
            {
                token.ThrowIfCancellationRequested();
                reader.ReadLine();
                lineCount++;
            }
            return lineCount;
        }

        public static DetectedLineEndings GetLineEndings(StreamReader reader, CancellationToken token)
        {
            int windowsCount = 0;
            int unixCount = 0;
            int oldMacCount = 0;
            int i, j;
            while ((i = reader.Read()) != -1)
            {
                token.ThrowIfCancellationRequested();
                char c = (char)i;
                if (c == '\r')
                {
                    if ((j = reader.Peek()) != -1)
                    {
                        if ((char)j == '\n')
                        {
                            windowsCount++;
                            reader.Read();
                        }
                        else
                        {
                            oldMacCount++;
                        }
                    }
                }
                else if (c == '\n')
                {
                    unixCount++;
                }
                int numTypes = 0;
                if (windowsCount > 0)
                {
                    numTypes++;
                }
                if (unixCount > 0)
                {
                    numTypes++;
                }
                if (oldMacCount > 0)
                {
                    numTypes++;
                }
                if (numTypes > 1)
                {
                    return DetectedLineEndings.Mixed;
                }
            }
            if (windowsCount == 0 && unixCount == 0 && oldMacCount == 0)
            {
                return DetectedLineEndings.NotApplicable;
            }
            else if (windowsCount > 0 && unixCount == 0 && oldMacCount == 0)
            {
                return DetectedLineEndings.Windows;
            }
            else if (windowsCount == 0 && unixCount > 0 && oldMacCount == 0)
            {
                return DetectedLineEndings.Unix;
            }
            else if (windowsCount == 0 && unixCount == 0 && oldMacCount > 0)
            {
                return DetectedLineEndings.OldMacOS;
            }
            else
            {
                return DetectedLineEndings.Mixed;
            }
        }
    }
}
