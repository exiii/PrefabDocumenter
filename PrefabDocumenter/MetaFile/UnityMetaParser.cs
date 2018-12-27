using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet;
using YamlDotNet.RepresentationModel;

namespace PrefabDocumenter
{
    public static class UnityMetaParser
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
            var guid = (YamlScalarNode)mapping.Children[new YamlScalarNode(UnityMetaKey.Guid)];
            var folderAsset = (YamlScalarNode)mapping.Children[new YamlScalarNode(UnityMetaKey.FolderAsset)];

            return new UnityMetaNode("", guid.Value, folderAsset.Value, "", "");
        }
    }
}
