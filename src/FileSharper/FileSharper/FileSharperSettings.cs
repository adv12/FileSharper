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

        public static string TemplatesDirectoryPath => Path.Combine(SettingsDirectoryPath, "Templates");

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
                InstallStockTemplates();
            }
            catch (Exception)
            {

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

        public static void InstallStockTemplates()
        {
            string sourceDirPath = Path.Combine(AppDirectoryPath, "Templates");
            string destDirPath = TemplatesDirectoryPath;
            EnsureTemplatesDirectoryPath();
            if (Directory.Exists(sourceDirPath) && Directory.Exists(destDirPath))
            {
                DirectoryInfo sourceDir = new DirectoryInfo(sourceDirPath);
                FileInfo[] files = sourceDir.GetFiles("*.fsh");
                foreach (FileInfo file in files)
                {
                    try
                    {
                        string destPath = Path.Combine(destDirPath, file.Name);
                        if (!File.Exists(destPath))
                        {
                            File.Copy(file.FullName, destPath);
                        }
                    }
                    catch (Exception)
                    {

                    }
                }
            }
        }

        public static void SyncTemplatesToFilesystem(FileSharperSettings settings)
        {
            EnsureTemplatesDirectoryPath();
            DirectoryInfo dir = new DirectoryInfo(TemplatesDirectoryPath);
            FileInfo[] templateFiles = dir.GetFiles("*.fsh");
            foreach (FileInfo file in templateFiles.OrderBy(f => f.Name))
            {
                if (!settings.Templates.Any(t => file.Name.Equals(t.FileName)))
                {
                    settings.Templates.Add(new SearchTemplateInfo(file.Name,
                        Path.GetFileNameWithoutExtension(file.FullName)));
                }
            }
            List<SearchTemplateInfo> toRemove = new List<SearchTemplateInfo>();
            foreach (SearchTemplateInfo template in settings.Templates)
            {
                string path = Path.Combine(TemplatesDirectoryPath, template.FileName);
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
            Directory.CreateDirectory(TemplatesDirectoryPath);
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
            string baseFilename = GetTemplateFullName(Regex.Replace(displayName, @"\W", ""));
            string filename = baseFilename;
            int j = 1;
            while (File.Exists(filename))
            {
                filename = baseFilename + j++;
            }
            doc.Save(filename);
            Templates.Add(new SearchTemplateInfo(filename, displayName));
        }

        public string GetTemplateFullName(string templateFileName)
        {
            return Path.Combine(TemplatesDirectoryPath, templateFileName);
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
