using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace PrefabDocumenter
{
    class XmlDocument
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="metaFileElement"></param>
        /// <returns></returns>
        public static async Task<XDocument> CreateDraftDocument(IEnumerable<XElement> metaFileElement)
        {
            var draftDocument = new XElement(XmlTags.MetaFilesTag);

            await Task.Run(() => {
                foreach (var element in metaFileElement)
                {
                    draftDocument.Add(new XElement(XmlTags.MetaFileTag,
                        new XAttribute(XmlTags.FileNameAttrTag, Regex.Split(element.Attribute(XmlTags.FilePathAttrTag).Value.ToString(), @"\\").Last()),
                        new XAttribute(XmlTags.GuidAttrTag, element.Attribute(XmlTags.GuidAttrTag).Value),
                        new XElement(XmlTags.DescriptionTag)));
                }
            });

            return new XDocument(new XDeclaration("1.0", "utf-8", null), draftDocument);
        }

        public static async Task<XDocument> UpdateDraftDocument(IEnumerable<XElement> metaFileElement, IEnumerable<XElement> draftDocumentElement)
        {
            var newDraftDocument = new XElement(XmlTags.MetaFilesTag);

            metaFileElement = metaFileElement.DescendantsAndSelf()
                                             .Where(element => element.Attribute(XmlTags.GuidAttrTag) != null);

            await Task.Run(() =>
            {
                try 
                {
                    foreach (var metaElement in metaFileElement) 
                    {
                        draftDocumentElement.DescendantsAndSelf()
                                            .Where(draftElement => draftElement.Attribute(XmlTags.GuidAttrTag) != null)
                                            .Where(draftElement => draftElement.Attribute(XmlTags.GuidAttrTag).Value == metaElement.Attribute(XmlTags.GuidAttrTag).Value)
                                            .ToList()
                                            .ForEach(draftElement => newDraftDocument.Add(draftElement));

                        var Exist = draftDocumentElement.DescendantsAndSelf()
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
                    newDraftDocument.Add(draftDocumentElement);
                }
            });

            return new XDocument(new XDeclaration("1.0", "utf-8", null), newDraftDocument);
        }
    }
}
