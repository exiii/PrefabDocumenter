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

            await Task.Run((Action)(() => {
                foreach (var path in metaFileSearcher.Search(FolderPath, FileNameFilterRegex))
                {
                    var relativePath = Regex.Replace(path, Regex.Escape(FolderPath), "");

                    var beforeElement = metaFileTreeXml;
                    foreach (var metaFileName in Regex.Split(relativePath, RegexTokens.PathSplit))
                    {
                        //TODO
                        //パスが""/aaa/bb/..."となっているので"/aaa/"の左側の部分が判定対象になっているため、殻の文字で来る。
                        if (metaFileName == "") 
                        {
                            continue;
                        }

                        var fileName = Regex.Replace(metaFileName, RegexTokens.MetaFileExtension, "");

                        if (beforeElement.DescendantsAndSelf().Attributes(XmlTags.FileNameAttr).Where(name => name.Value == fileName).Any() == false)
                        {

                            beforeElement.Add(new XElement(XmlTags.FileTag,
                                new XAttribute(XmlTags.FileNameAttr, fileName),
                                Regex.IsMatch(metaFileName, RegexTokens.MetaFileExtension) ? new XAttribute(XmlTags.FilePathAttr, relativePath) : null,
                                Regex.IsMatch(metaFileName, RegexTokens.MetaFileExtension) ? new XAttribute(XmlTags.GuidAttr, UnityMetaParser.Parse(new StreamReader(path).ReadToEnd()).Guid) : null,
                                Regex.IsMatch(metaFileName, RegexTokens.FileExtension) ? new XAttribute(XmlTags.TypeAttr, FileTypeValue.File) : new XAttribute(XmlTags.TypeAttr, FileTypeValue.Folder)
                                ));
                        }

                        beforeElement = beforeElement.Descendants()
                        .Where(element => 
                        {
                            var elementName = element.Attribute(XmlTags.FileNameAttr).Value;

                            return elementName == fileName;
                        }).First();

                        Console.WriteLine(beforeElement.Attribute(XmlTags.FileNameAttr).Value);
                        Console.WriteLine("---------------------------");
                    }
                }
            }));

            return new XDocument(new XDeclaration("1.0", "utf-8", null), metaFileTreeXml);
        }
    }
}
