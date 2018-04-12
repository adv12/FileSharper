// Copyright (c) 2018 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileSharperCore;
using Newtonsoft.Json;

namespace FileSharper
{
    public class FileSharperSettings
    {
        public static string SettingsDirectoryPath
        {
            get
            {
                string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                return Path.Combine(appData, "FileSharper");
            }
        }

        public static string SettingsPath => Path.Combine(SettingsDirectoryPath, "settings.json");

        public static FileSharperSettings Load()
        {
            try
            {
                string json = File.ReadAllText(SettingsPath);
                return JsonConvert.DeserializeObject<FileSharperSettings>(json);
            }
            catch (Exception ex)
            {
                return new FileSharperSettings();
            }
        }

        public static void EnsureSettingsDirectoryPath()
        {
            Directory.CreateDirectory(SettingsDirectoryPath);
        }

        public ObservableCollection<string> RecentDocuments { get; } =
            new ObservableCollection<string>();

        public SearchDocument NewSearchTemplate { get; set; }

        public void AddRecentDocument(string path)
        {
            RecentDocuments.Remove(path);
            RecentDocuments.Insert(0, path);
            while (RecentDocuments.Count > 10)
            {
                RecentDocuments.RemoveAt(RecentDocuments.Count - 1);
            }
        }

        private FileSharperSettings()
        {

        }

        public void Save()
        {
            Save(SettingsPath);
        }

        public void Save(string settingsPath)
        {
            EnsureSettingsDirectoryPath();
            string text = JsonConvert.SerializeObject(this);
            File.WriteAllText(settingsPath, text);
        }

    }
}
