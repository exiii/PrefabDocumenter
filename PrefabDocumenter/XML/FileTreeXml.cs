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
        const string targetFileExtension = @".*\.meta$";

        /// <summary>
        /// 
        /// </summary>
        /// <param name="FolderPath"></param>
        public static async Task<XDocument> CreateXElement(string FolderPath)
        {
            var metaFileTreeXml = new XElement(XmlTags.metaFilesTag, new XAttribute(XmlTags.selectFolderPathAttrTag, FolderPath));
            await Task.Run(() => {
                foreach (var path in Searcher.Search(FolderPath))
                {
                    var relativePath = Regex.Replace(path, Regex.Escape(FolderPath), "");

                    var beforeElement = metaFileTreeXml;
                    foreach (var fileName in Regex.Split(relativePath, pathSplitToken))
                    {
                        var replaceName = Regex.Replace(fileName, " ", "_");
                        replaceName = "_" + Regex.Replace(replaceName, @"[:|%|\(|\)|,|+|-|\[|\]]", "");

                        if (beforeElement.Element(replaceName) == null)
                        {
                            beforeElement.Add(new XElement(replaceName,
                                Regex.IsMatch(fileName, ":") ? new XAttribute("Drive", true) : null,
                                Regex.IsMatch(fileName, targetFileExtension) ? new XAttribute(XmlTags.filePathAttrTag, relativePath) : null,
                                Regex.IsMatch(fileName, targetFileExtension) ? new XAttribute(XmlTags.guidAttrTag, UnityMetaParser.Parse(new StreamReader(path).ReadToEnd()).Guid) : null
                                ));
                        }

                        beforeElement = beforeElement.Descendants(replaceName).First();
                    }
                }
            });

            return new XDocument(new XDeclaration("1.0", "utf-8", null), metaFileTreeXml);
        }
    }
}
