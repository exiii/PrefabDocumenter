using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet;
using YamlDotNet.RepresentationModel;

namespace exiii
{
    static class UnityMetaParser
    {
        /// <summary>
        /// FileFormatVersion, FolderAsset, TimeCreated and LicenseType isn't available.
        /// </summary>
        /// <param name="TargetSrting"></param>
        public static UnityMetaNode Parse(string TargetSrting)
        {
            var stringReader = new StringReader(TargetSrting);
            var yaml = new YamlStream();

            yaml.Load(stringReader);

            var mapping = (YamlMappingNode)yaml.Documents[0].RootNode;
            var guid = (YamlScalarNode)mapping.Children[new YamlScalarNode(UnityMetaNodeKey.Guid)];

            return new UnityMetaNode("", guid.Value, "", "", "");
        }
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

    class UnityMetaNodeKey
    {
        public const string Guid = "guid";
    }
}
