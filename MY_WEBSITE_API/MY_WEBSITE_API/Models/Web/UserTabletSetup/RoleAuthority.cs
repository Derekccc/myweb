using MY_WEBSITE_API.Models.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MY_WEBSITE_API.Models.Web.UserTabletSetup
{
    public class RoleAuthority
    {
        public string ROLE_ID { get; set; }
        public string ROLE_NAME { get; set; }
        public string MODULE_ID { get; set; }
        public string MODULE_NAME { get; set; }
        public string FEATURE_ID { get; set; }
        public string FEATURE_DESC { get; set; }
        public string STATION_ID { get; set; }
        public string STATION_DESC { get; set; }
        public string DEVICE_MST_ID { get; set; }
        public string DEVICE_DESC { get; set; }
        public string ROLE_AUTHORITY_ID { get; set; }
        public string AUTHORIZED { get; set; }
        public IEnumerable<RoleAuthorityDetails> AUTHORITY_DETAILS { get; set; }
        public UserActivity FROM_SOURCE { get; set; }
        public DateTime UPDATE_DATETIME { get; set; }
        public string UPDATE_ID { get; set; }
        public string UPDATE_NAME { get; set; }
    }
}