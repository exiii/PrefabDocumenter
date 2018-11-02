using PrefabDocumenter.XML;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace PrefabDocumenter.DB
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
            SqlDbProvider<PrefabDocumentModel>.DropTableCommnad = DropTableCommand;
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
                var enableDrafts = draftElements.DescendantsAndSelf(XMLTags.metaFileTag)
                                                .Where(element => element.Elements(XMLTags.metaFileTag) != null)
                                                .Where(element => element.Descendants(XMLTags.descriptionTag).First().Value != "");

                foreach (var descriptionElement in enableDrafts)
                {
                    metaFileElements.DescendantsAndSelf()
                                    .Where(metaElement => metaElement.Attribute(XMLTags.guidAttrTag) != null)
                                    .Where(metaElement => metaElement.Attribute(XMLTags.guidAttrTag).Value == descriptionElement.Attribute(XMLTags.guidAttrTag).Value)
                                    .ToList()
                                    .ForEach(element =>
                                    {
                                        string guid = element.Attribute(XMLTags.guidAttrTag).Value;
                                        string fileName = descriptionElement.Attribute(XMLTags.fileNameAttrTag).Value;
                                        string filePath = element.Attribute(XMLTags.filePathAttrTag).Value;
                                        string description = descriptionElement.Descendants(XMLTags.descriptionTag).First().Value;

                                        documentModels.Add(new PrefabDocumentModel(guid, fileName, filePath, description));
                                    });
                }
            });

            return documentModels;
        }
        
        //->
    }
}
