﻿using System;
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
            var draftDocument = new XElement(XmlTags.metaFilesTag);

            await Task.Run(() => {
                foreach (var element in metaFileElement)
                {
                    draftDocument.Add(new XElement(XmlTags.metaFileTag,
                        new XAttribute(XmlTags.fileNameAttrTag, Regex.Split(element.Attribute(XmlTags.filePathAttrTag).Value.ToString(), @"\\").Last()),
                        new XAttribute(XmlTags.guidAttrTag, element.Attribute(XmlTags.guidAttrTag).Value),
                        new XElement(XmlTags.descriptionTag)));
                }
            });

            return new XDocument(new XDeclaration("1.0", "utf-8", null), draftDocument);
        }

        public static async Task<XDocument> UpdateDraftDocument(IEnumerable<XElement> metaFileElement, IEnumerable<XElement> draftDocumentElement)
        {
            var newDraftDocument = new XElement(XmlTags.metaFilesTag);

            metaFileElement = metaFileElement.DescendantsAndSelf()
                                             .Where(element => element.Attribute(XmlTags.guidAttrTag) != null);

            await Task.Run(() =>
            {
                try 
                {
                    foreach (var metaElement in metaFileElement) 
                    {
                        draftDocumentElement.DescendantsAndSelf()
                                            .Where(draftElement => draftElement.Attribute(XmlTags.guidAttrTag) != null)
                                            .Where(draftElement => draftElement.Attribute(XmlTags.guidAttrTag).Value == metaElement.Attribute(XmlTags.guidAttrTag).Value)
                                            .ToList()
                                            .ForEach(draftElement => newDraftDocument.Add(draftElement));

                        var Exist = draftDocumentElement.DescendantsAndSelf()
                                                        .Where(draftElement => draftElement.Attribute(XmlTags.guidAttrTag) != null)
                                                        .Where(draftElement => draftElement.Attribute(XmlTags.guidAttrTag).Value == metaElement.Attribute(XmlTags.guidAttrTag).Value)
                                                        .Any();
                        if (!Exist) 
                        {
                            newDraftDocument.Add(new XElement(XmlTags.metaFileTag,
                                        new XAttribute(XmlTags.fileNameAttrTag, Regex.Split(metaElement.Attribute(XmlTags.filePathAttrTag).Value.ToString(), @"\\").Last()),
                                        new XAttribute(XmlTags.guidAttrTag, metaElement.Attribute(XmlTags.guidAttrTag).Value),
                                        new XElement(XmlTags.descriptionTag)));
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
