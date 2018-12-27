using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrefabDocumenter
{
    public class UnityMetaNode
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
