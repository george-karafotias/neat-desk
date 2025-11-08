using System.Collections.Generic;
using System.IO;

namespace NeatDesk
{
    public class FileOrganizer
    {
        private List<ILogger> loggers;

        public FileOrganizer(List<ILogger> loggers)
        {
            this.loggers = loggers;
        }

        public FileOrganizerResult OrganizeFiles(string folder, FileConfiguration fileConfiguration)
        {
            List<FileCategory> fileCategories = fileConfiguration.FileCategories;
            if (fileCategories == null || fileCategories.Count == 0)
            {
                return new FileOrganizerResult(false, "You have not configured any file categories. Please configure the file categories and retry.");
            }

            if (string.IsNullOrEmpty(folder))
            {
                return new FileOrganizerResult(false, "You have not selected any folder. Please select a folder and retry.");
            }

            if (!FolderExists(folder))
            {
                return new FileOrganizerResult(false, "The folder you selected does not exist anymore. Please select an existing folder and retry.");
            }

            List<string> files = GetFiles(folder);
            if (files == null)
            {
                return new FileOrganizerResult(false, "The selected folder cannot be read.");
            }
            if (files.Count == 0)
            {
                return new FileOrganizerResult(false, "No files were found under the selected folder.");
            }

            foreach (string file in files)
            {
                Log("Processing " + file + "...");

                FileCategory fileCategory = GetFileCategory(file, fileConfiguration);
                if (fileCategory == null) continue;
                string categoryFolder = Path.Combine(Path.GetDirectoryName(file), fileCategory.Name);
                MoveFile(file, categoryFolder);
            }

            return new FileOrganizerResult(true);
        }

        private List<string> GetFiles(string folder)
        {
            try
            {
                List<string> files = new List<string>();
                string[] allFiles = Directory.GetFiles(folder, "*.*", SearchOption.TopDirectoryOnly);
                foreach (string file in allFiles)
                {
                    files.Add(file);
                }
                return files;
            }
            catch
            {
                return null;
            }
        }

        private bool FolderExists(string folder) 
        {
            try
            {
                return Directory.Exists(folder);
            }
            catch
            {
                return false;
            }
        }

        private FileCategory GetFileCategory(string file, FileConfiguration fileConfiguration)
        {
            try
            {
                string extension = Path.GetExtension(file).ToLower();
                foreach (FileCategory fileCategory in fileConfiguration.FileCategories)
                {
                    foreach (string fileExtension in fileCategory.FileExtensions)
                    {
                        if (fileExtension.ToLower() == extension) return fileCategory;
                    }
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        private bool MoveFile(string sourceFilePath, string targetFolder)
        {
            try
            {
                Directory.CreateDirectory(targetFolder);
                string destFilePath = Path.Combine(targetFolder, Path.GetFileName(sourceFilePath));
                File.Move(sourceFilePath, destFilePath);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private void Log(string message)
        {
            if (loggers == null) return;
            foreach (ILogger logger in loggers)
            {
                logger.Log(message);
            }
        }
    }
}
