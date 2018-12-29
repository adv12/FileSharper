// Copyright (c) 2017 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace FileSharperCore.Util
{
    public class TextUtil
    {

        public static LineEndings GetLineEndings(LineEndingsNoFile lineEndings)
        {
            LineEndings endings = LineEndings.SystemDefault;
            switch (lineEndings)
            {
                case LineEndingsNoFile.SystemDefault:
                    endings = LineEndings.SystemDefault;
                    break;
                case LineEndingsNoFile.Windows:
                    endings = LineEndings.Windows;
                    break;
                case LineEndingsNoFile.Unix:
                    endings = LineEndings.Unix;
                    break;
                case LineEndingsNoFile.ClassicMacOS:
                    endings = LineEndings.ClassicMacOS;
                    break;
                default:
                    endings = LineEndings.SystemDefault;
                    break;
            }
            return endings;
        }

        public static string GetNewline(LineEndingsNoFile lineEndings)
        {
            return GetNewline(null, GetLineEndings(lineEndings));
        }

        public static string GetNewline(FileInfo file, LineEndings lineEndings)
        {
            switch (lineEndings)
            {
                case LineEndings.MatchInput:
                    if (file == null)
                    {
                        return Environment.NewLine;
                    }
                    using (StreamReader reader = new StreamReader(file.OpenRead()))
                    {
                        DetectedLineEndings detected = GetLineEndings(reader, true, CancellationToken.None);
                        switch (detected)
                        {
                            case DetectedLineEndings.Windows:
                                return "\r\n";
                            case DetectedLineEndings.Unix:
                                return "\n";
                            case DetectedLineEndings.ClassicMacOS:
                                return "\r";
                            default:
                                return Environment.NewLine;
                        }
                    }
                case LineEndings.Windows:
                    return "\r\n";
                case LineEndings.Unix:
                    return "\n";
                case LineEndings.ClassicMacOS:
                    return "\r";
                default:
                    return Environment.NewLine;
            }
        }

        public static bool FileEndsWithNewline(FileInfo file, Encoding encoding)
        {
            byte[] carriageReturn = encoding.GetBytes(new char[] { '\r' });
            byte[] lineFeed = encoding.GetBytes(new char[] { '\n' });
            byte[][] newlineBytes = new byte[][] { carriageReturn, lineFeed };

            using (FileStream fileStream = file.OpenRead())
            {
                foreach (byte[] bytes in newlineBytes)
                {
                    bool match = true;
                    fileStream.Seek(-bytes.Length, SeekOrigin.End);
                    for (int i = 0; i < bytes.Length; i++)
                    {
                        if (bytes[i] != fileStream.ReadByte())
                        {
                            match = false;
                            break;
                        }
                    }
                    if (match)
                    {
                        return true;
                    }
                }
            }
            return false;
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

        public static DetectedLineEndings GetLineEndings(StreamReader reader, bool fast, CancellationToken token)
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
                            if (fast)
                            {
                                break;
                            }
                        }
                        else
                        {
                            oldMacCount++;
                            if (fast)
                            {
                                break;
                            }
                        }
                    }
                }
                else if (c == '\n')
                {
                    unixCount++;
                    if (fast)
                    {
                        break;
                    }
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
                return DetectedLineEndings.ClassicMacOS;
            }
            else
            {
                return DetectedLineEndings.Mixed;
            }
        }

        public static Encoding DetectEncoding(FileInfo file)
        {
            string filename = file.FullName;
            Encoding encoding = null;
            try
            {
                using (FileStream fs = File.OpenRead(filename))
                {
                    Ude.CharsetDetector cdet = new Ude.CharsetDetector();
                    cdet.Feed(fs);
                    cdet.DataEnd();
                    if (cdet.Charset != null)
                    {
                        encoding = GetEncodingFromUdeCharset(cdet.Charset);
                    }
                }
            }
            catch (Exception)
            {
                // leave as null
            }
            if (encoding == null)
            {
                try
                {
                    using (StreamReader sr = new StreamReader(filename))
                    {
                        sr.Read();
                        encoding = sr.CurrentEncoding;
                    }
                }
                catch (IOException)
                {
                    // just return null
                }
            }
            return encoding;
        }

        public static Encoding GetEncodingFromUdeCharset(string charset)
        {
            // https://docs.microsoft.com/en-us/dotnet/api/system.text.encoding?view=netframework-4.7.1
            switch (charset)
            {
                case Ude.Charsets.ASCII:
                    return Encoding.ASCII;
                case Ude.Charsets.UTF8:
                    return Encoding.UTF8;
                case Ude.Charsets.UTF16_LE:
                    return Encoding.Unicode;
                case Ude.Charsets.UTF16_BE:
                    return Encoding.BigEndianUnicode;
                case Ude.Charsets.UTF32_BE:
                    return Encoding.GetEncoding(12001);
                case Ude.Charsets.UTF32_LE:
                    return Encoding.UTF32;
                case Ude.Charsets.WIN1252:
                    return Encoding.GetEncoding(1252); // Western European (Windows)
                case Ude.Charsets.EUCKR: // Korean (EUC)
                    return Encoding.GetEncoding(51949);
                case Ude.Charsets.EUCJP: // Japanese (EUC)
                    return Encoding.GetEncoding(51932);
                case Ude.Charsets.GB18030: // Chinese Simplified
                    return Encoding.GetEncoding(54936);
                case Ude.Charsets.ISO2022_JP: // Japanese (JIS)
                    return Encoding.GetEncoding(50220);
                case Ude.Charsets.ISO2022_CN: // Chinese Simplified (ISO-2022)
                    return Encoding.GetEncoding(50227);
                case Ude.Charsets.ISO2022_KR: // Korean (ISO)
                    return Encoding.GetEncoding(50225);
                case Ude.Charsets.HZ_GB_2312: // Chinese Simplified (HZ)
                    return Encoding.GetEncoding(52936);
                case Ude.Charsets.ISO8859_8: // Hebrew (ISO-Visual)
                    return Encoding.GetEncoding(28598);

                case Ude.Charsets.UCS4_3412: // not in Microsoft's list
                case Ude.Charsets.UCS4_2413: // not in Microsoft's list
                case Ude.Charsets.WIN1251: // Cyrillic (Windows), not supported by .NET
                case Ude.Charsets.WIN1253: // Greek (Windows), not supported by .NET
                case Ude.Charsets.WIN1255: // Hebrew (Windows), not supported by .NET
                case Ude.Charsets.BIG5: // Chinese Traditional, not supported by .NET
                case Ude.Charsets.EUCTW: // not in Microsoft's list
                case Ude.Charsets.SHIFT_JIS: // Japanese (Shift-JIS), not supported by .NET
                case Ude.Charsets.MAC_CYRILLIC: // Cyrillic (Mac), not supported by .NET
                case Ude.Charsets.KOI8R: // Cyrillic (KOI8-R), not supported by .NET
                case Ude.Charsets.IBM855: // OEM Cyrillic, not supported by .NET
                case Ude.Charsets.IBM866: // Cyrillic (DOS), not supported by .NET
                case Ude.Charsets.ISO8859_2: // Central European (ISO), not supported by .NET
                case Ude.Charsets.ISO8859_5: // Cyrillic (ISO), not supported by .NET
                case Ude.Charsets.ISO_8859_7: // Greek (ISO), not supported by .NET
                case Ude.Charsets.TIS620: // not in Microsoft's list
                    return null;
                default:
                    return null;
            }
        }

        public static StreamReader CreateStreamReaderWithAppropriateEncoding(FileInfo file, Encoding detectedEncoding)
        {
            if (detectedEncoding == null)
            {
                return new StreamReader(file.FullName);
            }
            return new StreamReader(file.FullName, detectedEncoding);
        }

        public static StreamWriter CreateStreamWriterWithAppropriateEncoding(string path,
            Encoding detectedEncoding, OutputEncodingType outputEncoding)
        {
            Encoding encoding = GetOutputEncoding(detectedEncoding, outputEncoding);
            if (encoding == null)
            {
                return new StreamWriter(path);
            }
            return new StreamWriter(path, false, encoding);
        }

        public static DetectedEncodingType GetDetectedEncodingType(FileInfo file)
        {
            Encoding encoding = DetectEncoding(file);
            try
            {
                int codePage = encoding.CodePage;
                return (DetectedEncodingType)codePage;
            }
            catch (Exception)
            {

            }
            return DetectedEncodingType.None;
        }

        public static Encoding GetOutputEncoding(Encoding detectedEncoding, OutputEncodingType outputEncodingType)
        {
            if (outputEncodingType == OutputEncodingType.MatchInput)
            {
                return detectedEncoding;
            }
            try
            {
                return Encoding.GetEncoding((int)outputEncodingType);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static Encoding GetSearchTextEncoding(SearchTextEncodingType searchTextEncodingType)
        {
            try
            {
                return Encoding.GetEncoding((int)searchTextEncodingType);
            }
            catch (Exception)
            {
                return null;
            }
            
        }

    }
}
