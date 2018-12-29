using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet;
using YamlDotNet.RepresentationModel;

namespace PrefabDocumenter.MetaFile
{
    public static class UnityMetaParser
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="TargetSrting"></param>
        public static UnityMeta Parse(string TargetSrting)
        {
            var stringReader = new StringReader(TargetSrting);
            var yaml = new YamlStream();

            yaml.Load(stringReader);

            var mapping = (YamlMappingNode)yaml.Documents[0].RootNode;
            var fileFormatVerGetResult = mapping.Children.TryGetValue(new YamlScalarNode(UnityMetaKey.FileFormatVersion), out var fileFormatVerNode);
            var guidNode = mapping.Children[new YamlScalarNode(UnityMetaKey.Guid)];
            var folderAssetGetResult = mapping.Children.TryGetValue(new YamlScalarNode(UnityMetaKey.FolderAsset), out var folderAssetNode);

            var fileFormatVerValue = fileFormatVerGetResult ?
                ((YamlScalarNode)fileFormatVerNode).Value :
                "";

            var guidValue = ((YamlScalarNode)guidNode).Value;

            var folderAssetValue = folderAssetGetResult ?
                ((YamlScalarNode)folderAssetNode).Value :
                "";

            return new UnityMeta(fileFormatVerValue, guidValue, folderAssetValue);
        }
    }
}
