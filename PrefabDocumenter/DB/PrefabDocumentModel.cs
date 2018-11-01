using PrefabDocumenter.XML;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace PrefabDocumenter.DB
{
    class PrefabDocumentModel : IPrefabDocumentModel
    {
        public static string CreateTableCommand
        {
            get
            {
                return $@"CREATE TABLE IF NOT EXISTS Document(" +
                        "guid TEXT PRIMARY KEY NOT NULL, " +
                        "filename TEXT NOT NULL, " +
                        "filepath TEXT NOT NULL, " +
                        "description TEXT NOT NULL);";
            } 
        }

        public string InsertCommand
        {
            get
            {
                return "INSERT INTO " +
                       $@"{TableName}(guid, filename, filepath, description) " +
                       $@"VALUES({Guid}, {FileName}, {FilePath}, {Description});";
            }
        }

        public string Guid { get; }

        public string FileName { get; }

        public string FilePath { get; }

        public string Description { get; }

        public string TableName
        {
            get
            {
                return "Document";
            }
        }

        //public const string TableName = "Documnet";


        public PrefabDocumentModel(string guid, string fileName, string filePath, string description)
        {
            Guid = guid;
            FileName = fileName;
            FilePath = fileName;
            Description = description;
        }

        //<- static method

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
                                        string fileName = element.Attribute(XMLTags.fileNameAttrTag).Value;
                                        string filePath = element.Attribute(XMLTags.filePathAttrTag).Value;
                                        string description = element.Attribute(XMLTags.descriptionTag).Value;

                                        documentModels.Add(new PrefabDocumentModel(guid, fileName, filePath, description));
                                    });
                }
            });

            return documentModels;
        }
        
        //->
    }
}
