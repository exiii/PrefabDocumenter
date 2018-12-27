using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrefabDocumenter
{
    public class UnityMeta
    {
        public string FileFormatVersion { private set; get; }
        public string Guid { private set; get; }
        public string FolderAsset { private set; get; }

        public UnityMeta(string FileFormatVersion, string Guid, string FolderAsset)
        {
            this.FileFormatVersion = FileFormatVersion;
            this.Guid = Guid;
            this.FolderAsset = FolderAsset;
        }
    }
}
