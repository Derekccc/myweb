using System;


namespace MY_WEBSITE_API.Models.Common
{
    public class UserActivity
    {
        public string GUID { get; set; }
        public string SOURCE { get; set; }
        public string MODULE_ID { get; set; }
        public string ACTION { get; set; }
        public string SYSTEM_MESSAGE { get; set; }
        public DateTime UPDATE_DATETIME { get; set; }
        public string UPDATE_BY { get; set; }
    }
}