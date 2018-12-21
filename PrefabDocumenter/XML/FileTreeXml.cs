using exiii;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace PrefabDocumenter
{
    public static class FileTreeXml
    {
        private const string pathSplitToken = @"\\";
        private const string metaFileExtension = @".*\.meta$";
        private const string fileExtension = @".+\..+$";

        private const string fileTypeValue = "File";
        private const string folderTypeValue = "Folder";

        /// <summary>
        /// 
        /// </summary>
        /// <param name="FolderPath"></param>
        public static async Task<XDocument> CreateXElement(string FolderPath, string FileNameFilterRegex = "")
        {
            var metaFileTreeXml = new XElement(XmlTags.MetaFilesTag, new XAttribute(XmlTags.SelectFolderPathAttrTag, FolderPath));

            var metaFileSearcher = new FileSearcher(metaFileExtension);

            await Task.Run(() => {
                foreach (var path in metaFileSearcher.Search(FolderPath, FileNameFilterRegex))
                {
                    var relativePath = Regex.Replace(path, Regex.Escape(FolderPath), "");

                    var beforeElement = metaFileTreeXml;
                    foreach (var fileName in Regex.Split(relativePath, pathSplitToken))
                    {
                        if (fileName == "") 
                        {
                            continue;
                        }

                        if (beforeElement.DescendantsAndSelf().Attributes(XmlTags.FileNameAttrTag).Where(name => name.Value == fileName).Any() == false)
                        {

                            beforeElement.Add(new XElement("File",
                                new XAttribute(XmlTags.FileNameAttrTag, fileName),
                                Regex.IsMatch(fileName, metaFileExtension) ? new XAttribute(XmlTags.FilePathAttrTag, relativePath) : null,
                                Regex.IsMatch(fileName, metaFileExtension) ? new XAttribute(XmlTags.GuidAttrTag, UnityMetaParser.Parse(new StreamReader(path).ReadToEnd()).Guid) : null,
                                Regex.IsMatch(fileName, fileExtension) ? new XAttribute(XmlTags.Type, fileTypeValue) : new XAttribute(XmlTags.Type, folderTypeValue)
                                ));
                        }

                        beforeElement = beforeElement.Descendants().Where(element => element.Attribute(XmlTags.FileNameAttrTag).Value == fileName).First();
                    }
                }
            });

            return new XDocument(new XDeclaration("1.0", "utf-8", null), metaFileTreeXml);
        }
    }
}
