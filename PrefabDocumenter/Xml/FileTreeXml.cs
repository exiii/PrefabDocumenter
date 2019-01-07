using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using PrefabDocumenter.RegexExtension;
using PrefabDocumenter.MetaFile;

namespace PrefabDocumenter.Xml
{
    public static class FileTreeXml
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="FolderPath"></param>
        public static async Task<XDocument> CreateXElement(string FolderPath, string FileNameFilterRegex = "")
        {
            var metaFileTreeXml = new XElement(XmlTags.MetaFilesTag, new XAttribute(XmlTags.SelectFolderPathAttr, FolderPath));

            var metaFileSearcher = new FileSearcher(RegexTokens.MetaFileExtension);

            await Task.Run(() => {
                foreach (var path in metaFileSearcher.Search(FolderPath, FileNameFilterRegex))
                {
                    var relativePath = Regex.Replace(path, Regex.Escape(FolderPath), "");

                    var beforeElement = metaFileTreeXml;
                    foreach (var fileName in Regex.Split(relativePath, RegexTokens.PathSplit))
                    {
                        //TODO
                        //パスが""/aaa/bb/..."となっているので"/aaa/"の左側の部分が判定対象になっているため、殻の文字で来る。
                        if (fileName == "") 
                        {
                            continue;
                        }

                        if (beforeElement.DescendantsAndSelf().Attributes(XmlTags.FileNameAttr).Where(name => name.Value == fileName).Any() == false)
                        {

                            beforeElement.Add(new XElement(XmlTags.FileTag,
                                new XAttribute(XmlTags.FileNameAttr, fileName),
                                Regex.IsMatch(fileName, RegexTokens.MetaFileExtension) ? new XAttribute(XmlTags.FilePathAttr, relativePath) : null,
                                Regex.IsMatch(fileName, RegexTokens.MetaFileExtension) ? new XAttribute(XmlTags.GuidAttr, UnityMetaParser.Parse(new StreamReader(path).ReadToEnd()).Guid) : null,
                                Regex.IsMatch(fileName, RegexTokens.FileExtension) ? new XAttribute(XmlTags.TypeAttr, FileTypeValue.File) : new XAttribute(XmlTags.TypeAttr, FileTypeValue.Folder)
                                ));
                        }

                        beforeElement = beforeElement.Descendants().Where(element => element.Attribute(XmlTags.FileNameAttr).Value == fileName).First();
                    }
                }
            });

            return new XDocument(new XDeclaration("1.0", "utf-8", null), metaFileTreeXml);
        }
    }
}
