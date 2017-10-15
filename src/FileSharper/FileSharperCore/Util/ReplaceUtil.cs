// Copyright (c) 2017 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace FileSharperCore.Util
{
    public class ReplaceUtil
    {
        public static Regex ReplacementRegex = new Regex(@"\{\s*([A-Za-z_][A-Za-z0-9_]*)\s*\}");
        public static string FilenameDateFormat = "yyyy-MM-dd-HH-mm-ss";

        public static string Replace(string input, FileInfo file)
        {
            Dictionary<string, string> replacements = new Dictionary<string, string>();
            replacements[nameof(file.Name)] = file.Name;
            replacements[nameof(file.FullName)] = file.FullName;
            replacements[nameof(file.DirectoryName)] = file.DirectoryName;
            replacements[nameof(file.Extension)] = file.Extension;
            replacements["NameMinusExtension"] = Path.GetFileNameWithoutExtension(file.Name);
            replacements[nameof(file.Length)] = file.Length.ToString();
            replacements[nameof(file.CreationTime)] = file.CreationTime.ToString(FilenameDateFormat);
            replacements[nameof(file.CreationTimeUtc)] = file.CreationTimeUtc.ToString(FilenameDateFormat);
            replacements[nameof(file.LastWriteTime)] = file.LastWriteTime.ToString(FilenameDateFormat);
            replacements[nameof(file.LastWriteTimeUtc)] = file.LastWriteTimeUtc.ToString(FilenameDateFormat);
            replacements[nameof(file.LastAccessTime)] = file.LastAccessTime.ToString(FilenameDateFormat);
            replacements[nameof(file.LastAccessTimeUtc)] = file.LastAccessTimeUtc.ToString(FilenameDateFormat);
            replacements[nameof(DateTime.Now)] = DateTime.Now.ToString(FilenameDateFormat);
            foreach (Environment.SpecialFolder folder in Enum.GetValues(typeof(Environment.SpecialFolder)))
            {
                replacements[folder.ToString()] = Environment.GetFolderPath(folder);
            }
            return Replace(input, replacements);
        }

        public static string Replace(string input, IDictionary<string, string> replacements)
        {
            MatchCollection matches = ReplacementRegex.Matches(input);
            StringBuilder sb = new StringBuilder();
            int startIndex = 0;
            foreach (Match match in matches)
            {
                sb.Append(input.Substring(startIndex, match.Index - startIndex));
                string varname = match.Groups[1].Value;
                string value = varname;
                if (replacements.ContainsKey(varname))
                {
                    value = replacements[varname];
                }
                sb.Append(value);
                startIndex = match.Index + match.Length;
            }
            sb.Append(input.Substring(startIndex, input.Length - startIndex));
            return sb.ToString();
        }
    }
}
