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
                        new XAttribute(XmlTags.FileNameAttrTag, Regex.Split(element.Attribute(XmlTags.FilePathAttrTag).Value.ToString(), @"\\").Last()),
                        new XAttribute(XmlTags.GuidAttrTag, element.Attribute(XmlTags.GuidAttrTag).Value),
                        new XElement(XmlTags.DescriptionTag)));
                }
            });

            return new XDocument(new XDeclaration("1.0", "utf-8", null), draftDocument);
        }

        public static async Task<XDocument> UpdateDraftDocument(IEnumerable<XElement> MetaFileElement, IEnumerable<XElement> DraftDocumentElement)
        {
            var newDraftDocument = new XElement(XmlTags.MetaFilesTag);

            MetaFileElement = MetaFileElement.DescendantsAndSelf()
                                             .Where(element => element.Attribute(XmlTags.GuidAttrTag) != null);

            await Task.Run(() =>
            {
                try 
                {
                    foreach (var metaElement in MetaFileElement) 
                    {
                        DraftDocumentElement.DescendantsAndSelf()
                                            .Where(draftElement => draftElement.Attribute(XmlTags.GuidAttrTag) != null)
                                            .Where(draftElement => draftElement.Attribute(XmlTags.GuidAttrTag).Value == metaElement.Attribute(XmlTags.GuidAttrTag).Value)
                                            .ToList()
                                            .ForEach(draftElement => newDraftDocument.Add(draftElement));

                        var Exist = DraftDocumentElement.DescendantsAndSelf()
                                                        .Where(draftElement => draftElement.Attribute(XmlTags.GuidAttrTag) != null)
                                                        .Where(draftElement => draftElement.Attribute(XmlTags.GuidAttrTag).Value == metaElement.Attribute(XmlTags.GuidAttrTag).Value)
                                                        .Any();
                        if (!Exist) 
                        {
                            newDraftDocument.Add(new XElement(XmlTags.MetaFileTag,
                                        new XAttribute(XmlTags.FileNameAttrTag, Regex.Split(metaElement.Attribute(XmlTags.FilePathAttrTag).Value.ToString(), @"\\").Last()),
                                        new XAttribute(XmlTags.GuidAttrTag, metaElement.Attribute(XmlTags.GuidAttrTag).Value),
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
