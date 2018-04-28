// Copyright (c) 2018 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using FileSharperCore;
using Newtonsoft.Json;

namespace FileSharper
{
    public class FileSharperSettings
    {
        public static string AppDirectoryPath => AppDomain.CurrentDomain.BaseDirectory;

        public static string AppDataPath => Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

        public static string SettingsDirectoryPath => Path.Combine(AppDataPath, "FileSharper");

        public static string SettingsPath => Path.Combine(SettingsDirectoryPath, "settings.json");

        public static string UserTemplatesDirectoryPath => Path.Combine(SettingsDirectoryPath, "Templates");

        public static string StockTemplatesDirectoryPath => Path.Combine(AppDirectoryPath, "Templates");

        public static string DefaultTemplatePath => Path.Combine(SettingsDirectoryPath, "defaultTemplate.fsh");

        public static FileSharperSettings Load()
        {
            FileSharperSettings settings = null;
            try
            {
                EnsureSettingsDirectoryPath();
            }
            catch (Exception)
            {

            }
            try
            {
                string json = File.ReadAllText(SettingsPath);
                settings = JsonConvert.DeserializeObject<FileSharperSettings>(json);
            }
            catch (Exception)
            {
                settings = new FileSharperSettings();
            }
            try
            {
                SyncTemplatesToFilesystem(settings);
            }
            catch (Exception)
            {

            }
            return settings;
        }

        public static void SyncTemplatesToFilesystem(FileSharperSettings settings)
        {
            EnsureTemplatesDirectoryPath();
            IEnumerable<FileInfo> templateFiles;
            DirectoryInfo dir;
            dir = new DirectoryInfo(StockTemplatesDirectoryPath);
            templateFiles = dir.GetFiles("*.fsh").OrderBy(f => f.Name);
            foreach (FileInfo file in templateFiles)
            {
                if (!settings.Templates.Any(t => t.Stock && file.Name.Equals(t.FileName)))
                {
                    string displayName = Path.GetFileNameWithoutExtension(file.Name);
                    displayName = Regex.Replace(displayName, @"^\d+\s*", string.Empty);
                    settings.Templates.Add(new SearchTemplateInfo(file.Name, displayName, true));
                }
            }
            dir = new DirectoryInfo(UserTemplatesDirectoryPath);
            templateFiles = dir.GetFiles("*.fsh").OrderBy(f => f.Name);
            foreach (FileInfo file in templateFiles)
            {
                if (!settings.Templates.Any(t => !t.Stock && file.Name.Equals(t.FileName)))
                {
                    settings.Templates.Add(new SearchTemplateInfo(file.Name,
                        Path.GetFileNameWithoutExtension(file.Name)));
                }
            }
            List<SearchTemplateInfo> toRemove = new List<SearchTemplateInfo>();
            foreach (SearchTemplateInfo template in settings.Templates)
            {
                string dirPath = template.Stock ? StockTemplatesDirectoryPath : UserTemplatesDirectoryPath;
                string path = Path.Combine(dirPath, template.FileName);
                if (!File.Exists(path))
                {
                    toRemove.Add(template);
                }
            }
            foreach (SearchTemplateInfo template in toRemove)
            {
                settings.Templates.Remove(template);
            }
        }

        public static void EnsureSettingsDirectoryPath()
        {
            Directory.CreateDirectory(SettingsDirectoryPath);
        }

        public static void EnsureTemplatesDirectoryPath()
        {
            Directory.CreateDirectory(UserTemplatesDirectoryPath);
        }

        public ObservableCollection<string> RecentDocuments { get; } =
            new ObservableCollection<string>();

        public ObservableCollection<SearchTemplateInfo> Templates { get; } =
            new ObservableCollection<SearchTemplateInfo>();

        public void AddRecentDocument(string path)
        {
            RecentDocuments.Remove(path);
            RecentDocuments.Insert(0, path);
            while (RecentDocuments.Count > 10)
            {
                RecentDocuments.RemoveAt(RecentDocuments.Count - 1);
            }
        }

        public void RemoveRecentDocument(string path)
        {
            RecentDocuments.Remove(path);
        }

        public void AddTemplate(SearchDocument doc, string displayName)
        {
            if (string.IsNullOrEmpty(displayName))
            {
                bool stop = false;
                string name = null;
                for (int i = 1; !stop; i++)
                {
                    name = "Template " + i;
                    stop = true;
                    foreach (SearchTemplateInfo info in Templates)
                    {
                        if (info.DisplayName == name)
                        {
                            stop = false;
                            break;
                        }
                    }
                }
            }
            // Use just letters, digits, and underscores for filenames
            string safeName = Regex.Replace(displayName, @"\W", "");
            string baseFilename = Path.Combine(UserTemplatesDirectoryPath, safeName);
            string filename = baseFilename + ".fsh";
            int j = 1;
            while (File.Exists(filename))
            {
                filename = baseFilename + j++ + ".fsh";
            }
            doc.Save(filename);
            Templates.Add(new SearchTemplateInfo(Path.GetFileName(filename), displayName));
        }

        public void SetDefaultTemplate(SearchDocument doc)
        {
            doc.Save(DefaultTemplatePath);
        }

        public void ResetDefaultTemplate()
        {
            File.Delete(DefaultTemplatePath);
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
            string text = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText(settingsPath, text);
        }

    }
}
