﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace PrefabDocumenter
{
    static class Searcher
    {
        const string targetFileExtension = @".*\.meta$";

        static public List<string> Search(string targetFolderPath, string FileNameFilterRegex = "")
        {
            var files = Directory.EnumerateFiles(targetFolderPath)
                            .Where(name => Regex.IsMatch(name, FileNameFilterRegex))
                            .Where(name => Regex.IsMatch(name, targetFileExtension))
                            .ToList();

            Directory.EnumerateDirectories(targetFolderPath).ToList().ForEach(path => files.AddRange(Search(path, FileNameFilterRegex)));

            return files;
        }
    }
}
