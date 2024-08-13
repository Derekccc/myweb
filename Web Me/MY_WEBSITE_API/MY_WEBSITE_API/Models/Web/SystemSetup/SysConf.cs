using MY_WEBSITE_API.Models.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MY_WEBSITE_API.Models.Web.SystemSetup
{
    public class Policy
    {
        public string POLICY_ID { get; set; }
        public string POLICY_DESC { get; set; }
        public string POLICY_VALUE { get; set; }
        public string ACTIVE_FLAG { get; set; }
        public string DELETE_FLAG { get; set; }
        public DateTime UPDATE_DATETIME { get; set; }
        public string UPDATE_BY { get; set; }
        public UserActivity FROM_SOURCE { get; set; }
    }
}