namespace MY_WEBSITE_API.Models.Common
{
    public class Error
    {
        public string METHOD_NAME { get; set; }
        public string TYPE { get; set; }
        public string MESSAGE { get; set; }
        public string Message { get; internal set; }
    }
}