using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using PrefabDocumenter.Xml;
using PrefabDocumenter.RegexExtension;

namespace PrefabDocumenter.Db
{
    class PrefabDocumentModel : IModel
    {
        public static readonly string CreateTableCommand = "CREATE TABLE IF NOT EXISTS Document(" +
                                                           $@"{DocumentColomnName.Guid} TEXT PRIMARY KEY NOT NULL, " +
                                                           $@"{DocumentColomnName.FileName} TEXT NOT NULL, " +
                                                           $@"{DocumentColomnName.FilePath} TEXT NOT NULL, " +
                                                           $@"{DocumentColomnName.IndentLevel} INT NOT NULL," +
                                                           $@"{DocumentColomnName.Description} TEXT);";

        public const string DropTableCommand = "DROP TABLE IF EXISTS Document;";

        public string InsertCommand =>
            $@"INSERT INTO {TableName} VALUES('{Guid}', '{@FileName}', '{@FilePath}', {IndentLevel.ToString()}, '{@Description}');";

        public const string TableName = "Document";

        public string Guid { get; }

        public string FileName { get; }

        public string FilePath { get; }

        public string Description { get; }

        public int IndentLevel { get; }


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
                                                    .Where(element => string.Intern(element.Descendants(XmlTags.DescriptionTag).First().Value) != "");

                    foreach (var descriptionElement in enableDrafts) {
                        MetaFileElements.DescendantsAndSelf()
                                        .Where(metaElement => metaElement.Attribute(XmlTags.GuidAttr) != null)
                                        .Where(metaElement => string.Intern(metaElement.Attribute(XmlTags.GuidAttr).Value) == string.Intern(descriptionElement.Attribute(XmlTags.GuidAttr).Value))
                                        .ToList()
                                        .ForEach(element => {
                                            var guid = string.Intern(element.Attribute(XmlTags.GuidAttr).Value);
                                            var fileName = string.Intern(descriptionElement.Attribute(XmlTags.FileNameAttr).Value);
                                            var filePath = string.Intern(Regex.Replace(element.Attribute(XmlTags.FilePathAttr).Value, RegexTokens.MetaFileExtension, ""));
                                            var description = string.Intern(descriptionElement.Descendants(XmlTags.DescriptionTag).First().Value);
                                            var indentLevel = element.Ancestors().Count() - 2;

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
