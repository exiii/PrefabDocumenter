using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrefabDocumenter.RegexExtension
{
    public static class RegexTokens
    {
        public const string PathSplit = @"\\";
        public const string MetaFileExtension = @".*\.meta$";
        public const string FileExtension = @".+\..+$";
    }
}
