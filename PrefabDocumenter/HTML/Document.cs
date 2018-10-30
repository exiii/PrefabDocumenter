using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using AngleSharp.Parser.Html;
using AngleSharp.Dom;
using AngleSharp.Dom.Html;
using PrefabDocumenter.XML;
using System.IO;

namespace PrefabDocumenter.HTML
{
    public class Document
    {
        private HtmlTemplate htmlTemplate;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="htmlContent"></param>
        public Document(string htmlContent)
        {
            htmlTemplate = new HtmlTemplate(htmlContent);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sr"></param>
        public Document(StreamReader sr)
        {
            htmlTemplate = new HtmlTemplate(sr);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="metaFileElement"></param>
        /// <returns></returns>
        public static async Task<XDocument> CreateDraftDocument(IEnumerable<XElement> metaFileElement)
        {
            var documentDraft = new XElement(XMLTags.metaFilesTag);

            await Task.Run(() => {
                foreach (var element in metaFileElement)
                {
                    documentDraft.Add(new XElement(XMLTags.metaFileTag,
                        new XAttribute(XMLTags.fileNameAttrTag, Regex.Split(element.Attribute(XMLTags.filePathAttrTag).Value.ToString(), @"\\").Last()),
                        new XAttribute(XMLTags.guidAttrTag, element.Attribute(XMLTags.guidAttrTag).Value),
                        new XElement(XMLTags.descriptionTag)));
                }
            });

            return new XDocument(new XDeclaration("1.0", "utf-8", null), documentDraft);
        }

        public async Task<IHtmlDocument> CreateDocument(XElement draftRootElement, XElement metaFileElementRoot)
        {
            //Console.WriteLine(metaFileElementRoot);
            return await CreateDocument(draftRootElement.Elements(), metaFileElementRoot.Elements());
        }

        public async Task<IHtmlDocument> CreateDocument(IEnumerable<XElement> draftElements, XElement metaFileRootElement)
        {
            return await CreateDocument(draftElements, metaFileRootElement.Elements());
        }

        public async Task<IHtmlDocument> CreateDocument(XElement draftRootElement, IEnumerable<XElement> metaFileElements)
        {
            return await CreateDocument(draftRootElement.Elements(), metaFileElements);
        }

        public async Task<IHtmlDocument> CreateDocument(IEnumerable<XElement> draftElements, IEnumerable<XElement> metaFileElements)
        {
            var enableDrafts = draftElements.DescendantsAndSelf(XMLTags.metaFileTag)
                .Where(element => element.Elements(XMLTags.metaFileTag) != null)
                .Where(element => element.Descendants(XMLTags.descriptionTag).First().Value != "");

            var parser = new HtmlParser();
            var document = parser.Parse(htmlTemplate.Content);

            await Task.Run(() => {
                foreach (var descriptionElement in enableDrafts)
                {
                    metaFileElements.DescendantsAndSelf()
                        .Where(metaElement => metaElement.Attribute(XMLTags.guidAttrTag) != null)
                        .Where(metaElement => metaElement.Attribute(XMLTags.guidAttrTag).Value == descriptionElement.Attribute(XMLTags.guidAttrTag).Value)
                        .ToList()
                        .ForEach(element => {
                            var htmlElements = new List<IElement>();

                            var h2 = document.CreateElement("h2");
                            h2.TextContent = Regex.Replace(descriptionElement.Attribute(XMLTags.fileNameAttrTag).Value, @".meta$", "");
                            htmlElements.Add(h2);

                            var pathPTag = document.CreateElement("p");
                            pathPTag.TextContent = "Path: " + Regex.Replace(element.Attribute(XMLTags.filePathAttrTag).Value, @".meta$", "");
                            htmlElements.Add(pathPTag);

                            var guidPTag = document.CreateElement("p");
                            guidPTag.TextContent = "Guid: " + element.Attribute(XMLTags.guidAttrTag).Value;
                            htmlElements.Add(guidPTag);

                            var descriptionPTag = document.CreateElement("p");
                            descriptionPTag.TextContent = descriptionElement.Descendants(XMLTags.descriptionTag).First().Value;
                            htmlElements.Add(descriptionPTag);

                            htmlElements.ForEach(htmlElement => document.Body.AppendChild(htmlElement));                        
                        });
                }

            });

            return document;
        }
    }
}
