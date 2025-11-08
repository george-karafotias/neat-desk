using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace NeatDesk
{
    public class FileConfiguration
    {
        private const string FILENAME = "NeatDeskConfig.txt";
        private const string DELIMITER = "=";
        public List<FileCategory> FileCategories {  get; set; }

        public FileConfiguration()
        {
            try
            {
                Dictionary<string, string> fileCategories = new Dictionary<string, string>();
            
                string[] lines = File.ReadAllLines(FILENAME);
                foreach (string line in lines)
                {
                    string[] lineParts = line.Split(new char[] { DELIMITER[0] });
                    if (lineParts == null || lineParts.Length != 2) continue;
                    fileCategories.Add(lineParts[0], lineParts[1]);
                }

                SetFileCategories(fileCategories);
            }
            catch
            {
                SetDefaultFileCategories();
            }
        }

        public FileConfiguration(List<FileCategory> fileCategories)
        {
            FileCategories = fileCategories;
        }

        public FileCategory GetFileCategory(string fileCategoryName)
        {
            foreach (FileCategory fileCategory in FileCategories)
            {
                if (fileCategory.Name == fileCategoryName)
                {
                    return fileCategory;
                }
            }

            return null;
        }

        public bool AddFileCategory(string fileCategoryName, List<string> fileExtensions)
        {
            FileCategory category = new FileCategory(fileCategoryName, NormalizeFileExtensions(fileExtensions));
            FileCategories.Add(category);
            return Save();
        }

        public bool UpdateFileCategory(string fileCategoryName, List<string> fileExtensions)
        {
            FileCategory fileCategory = GetFileCategory(fileCategoryName);
            if (fileCategory == null) return false;
            fileCategory.FileExtensions = NormalizeFileExtensions(fileExtensions);
            return Save();
        }

        public bool DeleteFileCategory(string fileCategoryName)
        {
            FileCategory fileCategory = GetFileCategory(fileCategoryName);
            if (fileCategory == null) return false;
            FileCategories.Remove(fileCategory);
            return Save();
        }

        private bool Save()
        {
            List<string> lines = new List<string>();
            foreach (FileCategory fileCategory in FileCategories)
            {
                fileCategory.FileExtensions = NormalizeFileExtensions(fileCategory.FileExtensions);
                lines.Add(fileCategory.Name + DELIMITER + string.Join(",", fileCategory.FileExtensions.ToArray()));
            }

            try
            {
                File.WriteAllLines(FILENAME, lines.ToArray());
            }
            catch
            {
                return false;
            }

            return true;
        }

        private List<string> NormalizeFileExtensions(List<string> fileExtensions)
        {
            List<string> normalizedFileExtensions = new List<string>();

            for (int i = 0; i < fileExtensions.Count; i++)
            {
                string fileExtension = fileExtensions[i].ToLower().Trim();
                if (!fileExtension.StartsWith("."))
                {
                    fileExtension = "." + fileExtension;
                }
                
                if (IsValidFileExtension(fileExtension))
                {
                    normalizedFileExtensions.Add(fileExtension);
                }
            }

            return normalizedFileExtensions;
        }

        private void SetDefaultFileCategories()
        {
            Dictionary<string, string> fileCategories = new Dictionary<string, string>();
            fileCategories.Add("Documents", ".pdf,.docx,.txt");
            fileCategories.Add("Images", ".png,.jpg,.jpeg,.gif");
            fileCategories.Add("Videos", ".mp4,.avi,.mov");
            fileCategories.Add("Archives", ".zip,.rar,.7z");
            SetFileCategories(fileCategories);
        }

        private void SetFileCategories(Dictionary<string, string> fileCategories)
        {
            FileCategories = new List<FileCategory>();
            if (fileCategories == null || fileCategories.Count == 0) return;

            foreach (KeyValuePair<string, string> fileCategory in fileCategories)
            {
                string[] fileExtensions = fileCategory.Value.Split(new char[] { ',' });
                if (fileExtensions == null || fileExtensions.Length == 0) return;
                FileCategories.Add(new FileCategory(fileCategory.Key, fileExtensions.ToList()));
            }
        }

        private bool IsValidFileExtension(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return false;

            input = input.Trim();

            // Must start with exactly one dot
            if (!input.StartsWith("."))
                return false;

            // Remove the first dot for checking the rest
            string rest = input.Substring(1);

            // Split by dot to handle compound extensions
            string[] parts = rest.Split('.');

            // Each part must be non-empty and alphanumeric
            foreach (var part in parts)
            {
                if (string.IsNullOrEmpty(part) || !part.All(char.IsLetterOrDigit))
                    return false;
            }

            return true;
        }
    }
}
