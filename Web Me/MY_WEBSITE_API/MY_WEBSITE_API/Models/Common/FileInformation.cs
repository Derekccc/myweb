using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MY_WEBSITE_API.Models.Common
{
    public class FileInformation
    {
        public string ATTACHMENT_PATH { get; set; }
        public string ATTACHMENT_NAME { get; set; }
        public string FILE_NAME { get; set; }
        public string FILE_TITLE { get; set; }
        public string FILE_TYPE { get; set; }
        public string FILE_EXTENSION { get; set; }
        public string FILE_DATA_BASE64 { get; set; }
        public string FILE_PATH { get; set; }
        public string FILE_TEMP_PATH { get; set; }
        public List<string> UPLOAD_DATA { get; set; }
        //public List<AssemblyImage> UPLOAD_PROCESS_DATA { get; set; }
    }
}