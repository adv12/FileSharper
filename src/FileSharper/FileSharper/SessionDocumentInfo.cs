// Copyright (c) 2026 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System.IO;
using Newtonsoft.Json;

namespace FileSharper
{
    /// <summary>
    /// Describes a tab that was open when the app last closed.  The tab's
    /// current contents are stored in the session directory (so unsaved
    /// searches and unsaved edits survive), and FileName remembers the
    /// file the tab was saved to, if any.
    /// </summary>
    public class SessionDocumentInfo
    {
        public string SessionFileName { get; }

        public string FileName { get; }

        [JsonIgnore]
        public string SessionFileFullName => Path.Combine(FileSharperSettings.SessionDirectoryPath, SessionFileName);

        public SessionDocumentInfo(string sessionFileName, string fileName)
        {
            SessionFileName = sessionFileName;
            FileName = fileName;
        }
    }
}
