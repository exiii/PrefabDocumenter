using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace exiii
{
    static class UnityMetaParser
    {
        const string guidKey = "guid";

        /// <summary>
        /// FileFormatVersion, FolderAsset, TimeCreated and LicenseType isn't available.
        /// </summary>
        /// <param name="targetSrting"></param>
        public static UnityMetaNode Parse(string targetSrting)
        {
            var stringReader = new StringReader(targetSrting);
            var yaml = new YamlStream();

            yaml.Load(stringReader);

            var mapping = (YamlMappingNode)yaml.Documents[0].RootNode;
            var guid = (YamlScalarNode)mapping.Children[new YamlScalarNode(guidKey)];

            return new UnityMetaNode("", guid.Value, "", "", "");
        }

        /*
        public static UnityMetaNode StreamParse(string path)
        {

        }

        public static UnityMetaNode StreamParse(TextReader stream)
        {

        }
        */
    }

    class UnityMetaNode
    {
        public string FileFormatVersion { private set; get; }
        public string Guid { private set; get; }
        public string FolderAsset { private set; get; }
        public string TimeCreated { private set; get; }
        public string LicenseType { private set; get; }

        public UnityMetaNode(string FileFormatVersion, string Guid, string FolderAsset, string TimeCreated, string LicenseType)
        {
            this.FileFormatVersion = FileFormatVersion;
            this.Guid = Guid;
            this.FolderAsset = FolderAsset;
            this.TimeCreated = TimeCreated;
            this.LicenseType = LicenseType;
        }
    }
}
