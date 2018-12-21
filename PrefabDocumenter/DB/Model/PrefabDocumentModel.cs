using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
                                                  "indentLevel INT NOT NULL," +
                                                  "description TEXT);";

        public const string DropTableCommand = "DROP TABLE IF EXISTS Document;";

        public string InsertCommand
        {
            get
            {
                return $@"INSERT INTO {TableName} " +
                       $@"VALUES('{Guid}', '{@FileName}', '{@FilePath}', {IndentLevel}, '{@Description}');";
            }
        }

        public string Guid { get; }

        public string FileName { get; }

        public string FilePath { get; }

        public string Description { get; }

        public int IndentLevel { get; }

        public const string TableName = "Document";

        public PrefabDocumentModel(string Guid, string FileName, string FilePath, string Description, int IndentLevel)
        {
            this.Guid = Guid;
            this.FileName = FileName;
            this.FilePath = FilePath;
            this.Description = Description;
            this.IndentLevel = IndentLevel;
        }

        //<- static method
        static PrefabDocumentModel()
        {
            SqlDbProvider<PrefabDocumentModel>.DropTableCommand = DropTableCommand;
            SqlDbProvider<PrefabDocumentModel>.CreateTableCommand = CreateTableCommand;
        }

        public static async Task<List<PrefabDocumentModel>> CreateXmlToModel(XElement DraftRootElement, XElement MetaFileElementRoot)
        {
            return await CreateXmlToModel(DraftRootElement.Elements(), MetaFileElementRoot.Elements());
        }

        public static async Task<List<PrefabDocumentModel>> CreateXmlToModel(IEnumerable<XElement> DraftElements, XElement MetaFileRootElement)
        {
            return await CreateXmlToModel(DraftElements, MetaFileRootElement.Elements());
        }

        public static async Task<List<PrefabDocumentModel>> CreateXmlToModel(XElement DraftRootElement, IEnumerable<XElement> MetaFileElements)
        {
            return await CreateXmlToModel(DraftRootElement.Elements(), MetaFileElements);
        }

        public static async Task<List<PrefabDocumentModel>> CreateXmlToModel(IEnumerable<XElement> DraftElements, IEnumerable<XElement> MetaFileElements)
        {
            var documentModels = new List<PrefabDocumentModel>();

            await Task.Run(() => 
            {
                try {
                    var enableDrafts = DraftElements.DescendantsAndSelf(XmlTags.MetaFileTag)
                                                    .Where(element => element.Elements(XmlTags.MetaFileTag) != null)
                                                    .Where(element => element.Descendants(XmlTags.DescriptionTag).First().Value != "");

                    foreach (var descriptionElement in enableDrafts) {
                        MetaFileElements.DescendantsAndSelf()
                                        .Where(metaElement => metaElement.Attribute(XmlTags.GuidAttrTag) != null)
                                        .Where(metaElement => metaElement.Attribute(XmlTags.GuidAttrTag).Value == descriptionElement.Attribute(XmlTags.GuidAttrTag).Value)
                                        .ToList()
                                        .ForEach(element => {
                                            string guid = element.Attribute(XmlTags.GuidAttrTag).Value;
                                            string fileName = Regex.Replace(descriptionElement.Attribute(XmlTags.FileNameAttrTag).Value, ".meta$", "");
                                            string filePath = Regex.Replace(element.Attribute(XmlTags.FilePathAttrTag).Value, ".meta$", ""); ;
                                            string description = descriptionElement.Descendants(XmlTags.DescriptionTag).First().Value;
                                            int indentLevel = element.Ancestors().Count() - 2;

                                            documentModels.Add(new PrefabDocumentModel(guid, fileName, filePath, description, indentLevel));
                                        });
                    }
                }
                catch
                {
                    documentModels = new List<PrefabDocumentModel>();
                }
            });

            return documentModels;
        }
        
        //->
    }
}
