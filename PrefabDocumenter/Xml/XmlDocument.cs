using PrefabDocumenter.RegexExtension;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace PrefabDocumenter.Xml
{
    class XmlDocument
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="MetaFileElement"></param>
        /// <returns></returns>
        public static async Task<XDocument> CreateDraftDocument(IEnumerable<XElement> MetaFileElement)
        {
            var draftDocument = new XElement(XmlTags.MetaFilesTag);

            await Task.Run(() => {
                foreach (var element in MetaFileElement)
                {
                    draftDocument.Add(new XElement(XmlTags.MetaFileTag,
                        new XAttribute(XmlTags.FileNameAttr, string.Intern(element.Attribute(XmlTags.FileNameAttr).Value)),
                        new XAttribute(XmlTags.GuidAttr, string.Intern(element.Attribute(XmlTags.GuidAttr).Value)),
                        new XElement(XmlTags.DescriptionTag)));
                }
            });

            return new XDocument(new XDeclaration("1.0", "utf-8", null), draftDocument);
        }

        public static async Task<XDocument> UpdateDraftDocument(IEnumerable<XElement> MetaFileElement, IEnumerable<XElement> DraftDocumentElement)
        {
            var newDraftDocument = new XElement(XmlTags.MetaFilesTag);

            MetaFileElement = MetaFileElement.DescendantsAndSelf()
                                             .Where(element => element.Attribute(XmlTags.GuidAttr) != null);

            await Task.Run(() =>
            {
                try 
                {
                    foreach (var metaElement in MetaFileElement) 
                    {
                        DraftDocumentElement.DescendantsAndSelf()
                                            .Where(draftElement => draftElement.Attribute(XmlTags.GuidAttr) != null)
                                            .Where(draftElement => string.Intern(draftElement.Attribute(XmlTags.GuidAttr).Value) == string.Intern(metaElement.Attribute(XmlTags.GuidAttr).Value))
                                            .ToList()
                                            .ForEach(draftElement => newDraftDocument.Add(draftElement));

                        var Exist = DraftDocumentElement.DescendantsAndSelf()
                            .Where(draftElement => draftElement.Attribute(XmlTags.GuidAttr) != null)
                            .Any(draftElement =>
                                string.Intern(draftElement.Attribute(XmlTags.GuidAttr).Value) ==
                                string.Intern(metaElement.Attribute(XmlTags.GuidAttr).Value));
                        
                        if (!Exist) 
                        {
                            newDraftDocument.Add(new XElement(XmlTags.MetaFileTag,
                                        new XAttribute(XmlTags.FileNameAttr, Regex.Split(String.Intern(metaElement.Attribute(XmlTags.FilePathAttr).Value), RegexTokens.PathSplit).Last()),
                                        new XAttribute(XmlTags.GuidAttr, string.Intern(metaElement.Attribute(XmlTags.GuidAttr).Value)),
                                        new XElement(XmlTags.DescriptionTag)));
                        }
                    }
                }
                catch 
                {
                    newDraftDocument.Add(DraftDocumentElement);
                }
            });

            return new XDocument(new XDeclaration("1.0", "utf-8", null), newDraftDocument);
        }
    }
}
