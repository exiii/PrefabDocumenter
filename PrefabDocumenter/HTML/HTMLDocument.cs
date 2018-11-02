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
using System.IO;

namespace PrefabDocumenter
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
            var enableDrafts = draftElements.DescendantsAndSelf(XmlTags.metaFileTag)
                .Where(element => element.Elements(XmlTags.metaFileTag) != null)
                .Where(element => element.Descendants(XmlTags.descriptionTag).First().Value != "");

            var parser = new HtmlParser();
            var document = parser.Parse(htmlTemplate.Content);

            await Task.Run(() => {
                foreach (var descriptionElement in enableDrafts)
                {
                    metaFileElements.DescendantsAndSelf()
                        .Where(metaElement => metaElement.Attribute(XmlTags.guidAttrTag) != null)
                        .Where(metaElement => metaElement.Attribute(XmlTags.guidAttrTag).Value == descriptionElement.Attribute(XmlTags.guidAttrTag).Value)
                        .ToList()
                        .ForEach(element => {
                            var htmlElements = new List<IElement>();

                            var h2 = document.CreateElement("h2");
                            h2.TextContent = Regex.Replace(descriptionElement.Attribute(XmlTags.fileNameAttrTag).Value, @".meta$", "");
                            htmlElements.Add(h2);

                            var pathPTag = document.CreateElement("p");
                            pathPTag.TextContent = "Path: " + Regex.Replace(element.Attribute(XmlTags.filePathAttrTag).Value, @".meta$", "");
                            htmlElements.Add(pathPTag);

                            var guidPTag = document.CreateElement("p");
                            guidPTag.TextContent = "Guid: " + element.Attribute(XmlTags.guidAttrTag).Value;
                            htmlElements.Add(guidPTag);

                            var descriptionPTag = document.CreateElement("p");
                            descriptionPTag.TextContent = descriptionElement.Descendants(XmlTags.descriptionTag).First().Value;
                            htmlElements.Add(descriptionPTag);

                            htmlElements.ForEach(htmlElement => document.Body.AppendChild(htmlElement));                        
                        });
                }

            });

            return document;
        }
    }
}
