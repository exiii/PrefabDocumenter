using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace PrefabDocumenter.MetaFile
{
    public class FileSearcher
    {
        public string TargetFileExtension { get; private set; }

        public FileSearcher(string TargetFileExtension) 
        {
            this.TargetFileExtension = TargetFileExtension;
        }

        public List<string> Search(string TargetFolderPath, string FileNameFilterRegex = "")
        {
            var files = Directory.EnumerateFiles(TargetFolderPath)
                            .Where(name => Regex.IsMatch(name, FileNameFilterRegex))
                            .Where(name => Regex.IsMatch(name, TargetFileExtension))
                            .ToList();

            Directory.EnumerateDirectories(TargetFolderPath).ToList().ForEach(path => files.AddRange(Search(path, FileNameFilterRegex)));

            return files;
        }
    }
}
