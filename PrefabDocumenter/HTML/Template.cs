using System.IO;

namespace PrefabDocumenter.HTML
{
    class HtmlTemplate
    {
        public string Content;

        public HtmlTemplate()
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Content"></param>
        public HtmlTemplate(string Content)
        {
            this.Content = Content;
        }

        public HtmlTemplate(StreamReader sr)
        {
            this.Content = sr.ReadToEnd();
        }
    }
}
