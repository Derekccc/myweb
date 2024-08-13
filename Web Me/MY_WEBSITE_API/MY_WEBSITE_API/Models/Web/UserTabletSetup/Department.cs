using MY_WEBSITE_API.Models.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MY_WEBSITE_API.Models.Web.UserTabletSetup
{
    public class Department
    {
        public string DEPARTMENT_ID { get; set; }
        public string DEPARTMENT_NAME { get; set; }
        public string DEPARTMENT_DESC { get; set; }
        public string DEPARTMENT_ACTIVE_FLAG { get; set; }
        public string DEPARTMENT_DELETE_FLAG { get; set; }
        public DateTime UPDATE_DATETIME { get; set; }
        public string UPDATE_ID { get; set; }
        public string UPDATE_NAME { get; set; }
        public UserActivity FROM_SOURCE { get; set; }
        public bool DUPLICATE_DEPARTMENT_NAME { get; set; }
    }
}