using System.Collections.Generic;

namespace NeatDesk
{
    public class FileCategory
    {
        public string Name { get; set; }
        public List<string> FileExtensions { get; set; }
    
        public FileCategory(string name, List<string> fileExtensions)
        {
            Name = name;
            FileExtensions = fileExtensions;
        }
    }
}
