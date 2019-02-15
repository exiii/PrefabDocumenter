using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrefabDocumenter.MetaFile
{
    public class UnityMeta
    {
        public string FileFormatVersion { get; }
        public string Guid { get; }
        public string FolderAsset { get; }

        public UnityMeta(string FileFormatVersion, string Guid, string FolderAsset)
        {
            this.FileFormatVersion = FileFormatVersion;
            this.Guid = Guid;
            this.FolderAsset = FolderAsset;
        }
    }
}
