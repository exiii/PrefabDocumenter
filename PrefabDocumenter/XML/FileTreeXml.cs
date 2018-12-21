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
        const string pathSplitToken = @"\\";

        /// <summary>
        /// 
        /// </summary>
        /// <param name="FolderPath"></param>
        public static async Task<XDocument> CreateXElement(string FolderPath, string TargetFileExtension, string FileNameFilterRegex = "")
        {
            var metaFileTreeXml = new XElement(XmlTags.MetaFilesTag, new XAttribute(XmlTags.SelectFolderPathAttrTag, FolderPath));

            var metaFileSearcher = new FileSearcher(TargetFileExtension);

            await Task.Run(() => {
                foreach (var path in metaFileSearcher.Search(FolderPath, FileNameFilterRegex))
                {
                    var relativePath = Regex.Replace(path, Regex.Escape(FolderPath), "");

                    var beforeElement = metaFileTreeXml;
                    foreach (var fileName in Regex.Split(relativePath, pathSplitToken))
                    {

                        if (beforeElement.DescendantsAndSelf().Attributes(XmlTags.FileNameAttrTag).Where(name => name.Value == fileName).Any() == false)
                        {
                            Console.WriteLine(relativePath);

                            beforeElement.Add(new XElement("File",
                                new XAttribute(XmlTags.FileNameAttrTag, fileName),
                                Regex.IsMatch(fileName, TargetFileExtension) ? new XAttribute(XmlTags.FilePathAttrTag, relativePath) : null,
                                Regex.IsMatch(fileName, TargetFileExtension) ? new XAttribute(XmlTags.GuidAttrTag, UnityMetaParser.Parse(new StreamReader(path).ReadToEnd()).Guid) : null
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
