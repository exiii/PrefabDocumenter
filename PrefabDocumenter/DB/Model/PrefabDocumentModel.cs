using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace PrefabDocumenter
{
    class PrefabDocumentModel : IModel
    {
        public const string CreateTableCommand = "CREATE TABLE IF NOT EXISTS Document(" +
                                                  "guid TEXT PRIMARY KEY NOT NULL, " +
                                                  "filename TEXT NOT NULL, " +
                                                  "filepath TEXT NOT NULL, " +
                                                  "description TEXT);";

        public const string DropTableCommand = "DROP TABLE IF EXISTS Document;";

        public string InsertCommand
        {
            get
            {
                return $@"INSERT INTO {TableName} " +
                       $@"VALUES('{Guid}', '{@FileName}', '{@FilePath}', '{@Description}');";
            }
        }

        public string Guid { get; }

        public string FileName { get; }

        public string FilePath { get; }

        public string Description { get; }

        public const string TableName = "Document";

        public PrefabDocumentModel(string guid, string fileName, string filePath, string description)
        {
            Guid = guid;
            FileName = fileName;
            FilePath = fileName;
            Description = description;
        }

        //<- static method
        static PrefabDocumentModel()
        {
            SqlDbProvider<PrefabDocumentModel>.DropTableCommand = DropTableCommand;
            SqlDbProvider<PrefabDocumentModel>.CreateTableCommand = CreateTableCommand;
        }

        public static async Task<List<PrefabDocumentModel>> CreateXmlToModel(XElement draftRootElement, XElement metaFileElementRoot)
        {
            return await CreateXmlToModel(draftRootElement.Elements(), metaFileElementRoot.Elements());
        }

        public static async Task<List<PrefabDocumentModel>> CreateXmlToModel(IEnumerable<XElement> draftElements, XElement metaFileRootElement)
        {
            return await CreateXmlToModel(draftElements, metaFileRootElement.Elements());
        }

        public static async Task<List<PrefabDocumentModel>> CreateXmlToModel(XElement draftRootElement, IEnumerable<XElement> metaFileElements)
        {
            return await CreateXmlToModel(draftRootElement.Elements(), metaFileElements);
        }

        public static async Task<List<PrefabDocumentModel>> CreateXmlToModel(IEnumerable<XElement> draftElements, IEnumerable<XElement> metaFileElements)
        {
            var documentModels = new List<PrefabDocumentModel>();

            await Task.Run(() => 
            {
                var enableDrafts = draftElements.DescendantsAndSelf(XmlTags.metaFileTag)
                                                .Where(element => element.Elements(XmlTags.metaFileTag) != null)
                                                .Where(element => element.Descendants(XmlTags.descriptionTag).First().Value != "");

                foreach (var descriptionElement in enableDrafts)
                {
                    metaFileElements.DescendantsAndSelf()
                                    .Where(metaElement => metaElement.Attribute(XmlTags.guidAttrTag) != null)
                                    .Where(metaElement => metaElement.Attribute(XmlTags.guidAttrTag).Value == descriptionElement.Attribute(XmlTags.guidAttrTag).Value)
                                    .ToList()
                                    .ForEach(element =>
                                    {
                                        string guid = element.Attribute(XmlTags.guidAttrTag).Value;
                                        string fileName = descriptionElement.Attribute(XmlTags.fileNameAttrTag).Value;
                                        string filePath = element.Attribute(XmlTags.filePathAttrTag).Value;
                                        string description = descriptionElement.Descendants(XmlTags.descriptionTag).First().Value;

                                        documentModels.Add(new PrefabDocumentModel(guid, fileName, filePath, description));
                                    });
                }
            });

            return documentModels;
        }
        
        //->
    }
}
