// Copyright (c) 2017 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace FileSharperCore.Util
{
    public class ReplaceUtil
    {
        private static readonly Regex ReplacementRegex = new Regex(@"\{\s*([A-Za-z_][A-Za-z0-9_]*)\s*\}");
        private static readonly string FilenameDateTimeFormat = "yyyy-MM-dd-HH-mm-ss";
        private static readonly string FilenameDateOnlyFormat = "yyyy-MM-dd";

        public static string Replace(string input, FileInfo file)
        {
            Dictionary<string, string> replacements = new Dictionary<string, string>();
            if (file != null)
            {
                replacements[nameof(file.Name)] = file.Name;
                replacements[nameof(file.FullName)] = file.FullName;
                replacements[nameof(file.DirectoryName)] = file.DirectoryName;
                replacements[nameof(file.Extension)] = file.Extension;
                replacements["NameWithoutExtension"] = Path.GetFileNameWithoutExtension(file.Name);
                replacements["NameMinusExtension"] = replacements["NameWithoutExtension"];
                DirectoryInfo directory = file.Directory;
                int i = 1;
                while (directory != null)
                {
                    replacements[$"ParentName{i++}"] = directory.Name;
                    directory = directory.Parent;
                }
                if (file.Exists)
                {
                    replacements[nameof(file.Length)] = file.Length.ToString();
                    replacements[nameof(file.CreationTime)] = file.CreationTime.ToString(FilenameDateTimeFormat);
                    replacements[nameof(file.CreationTimeUtc)] = file.CreationTimeUtc.ToString(FilenameDateTimeFormat);
                    replacements[nameof(file.LastWriteTime)] = file.LastWriteTime.ToString(FilenameDateTimeFormat);
                    replacements[nameof(file.LastWriteTimeUtc)] = file.LastWriteTimeUtc.ToString(FilenameDateTimeFormat);
                    replacements[nameof(file.LastAccessTime)] = file.LastAccessTime.ToString(FilenameDateTimeFormat);
                    replacements[nameof(file.LastAccessTimeUtc)] = file.LastAccessTimeUtc.ToString(FilenameDateTimeFormat);
                }
            }
            replacements[nameof(DateTime.Now)] = DateTime.Now.ToString(FilenameDateTimeFormat);
            replacements[nameof(DateTime.Today)] = DateTime.Today.ToString(FilenameDateOnlyFormat);
            replacements[nameof(DateTime.Now.Date)] = DateTime.Now.Date.ToString(FilenameDateOnlyFormat);
            foreach (Environment.SpecialFolder folder in Enum.GetValues(typeof(Environment.SpecialFolder)))
            {
                replacements[folder.ToString()] = Environment.GetFolderPath(folder);
            }
            return Replace(input, replacements);
        }

        public static string Replace(string input, IDictionary<string, string> replacements)
        {
            return ReplacementRegex.Replace(input,
                m =>
                {
                    string varname = m.Groups[1].Value;
                    string value = varname;
                    if (replacements.ContainsKey(varname))
                    {
                        value = replacements[varname];
                    }
                    return value;
                });
        }
    }
}
