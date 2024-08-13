using MY_WEBSITE_API.Models.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MY_WEBSITE_API.Models.Web.ConsumerTabletSetup
{
    public class Customer
    {
        public string CUSTOMER_ID { get; set; }
        public string CUSTOMER_NAME { get; set; }
        public string PASSWORD { get; set; }
        public string DEFAULT_PASSWORD { get; set; }
        public string EMAIL { get; set; }
        public string PHONE_NO { get; set; }
        public string ADDRESS { get; set; }
        public string COMPANY_NAME { get; set; }
        public string USERROLE_ID { get; set; }
        public string CUSTOMER_CATEGORY { get; set; }
        public string ROLE_ID { get; set; }
        public string ROLE_NAME { get; set; }
        public string EXPIRE_FLAG { get; set; }
        public string ACCOUNT_LOCK_FLAG { get; set; }
        public DateTime LAST_LOG_IN { get; set; }
        public DateTime UPDATE_DATETIME { get; set; }
        public string UPDATE_ID { get; set; }
        public string ACTIVE_FLAG { get; set; }
        public string DELETE_FLAG { get; set; }
        public string RESET_FLAG { get; set; }
        public DateTime LAST_ACCESS_DATETIME { get; set; }
        public string ACTION { get; set; }
        public bool DUPLICATE_CUSTOMER_ID { get; set; }
        public bool DUPLICATE_CUSTOMER_NAME { get; set; }
        public bool DUPLICATE_CUSTOMER_EMAIL { get; set; }
        public bool DUPLICATE_CUSTOMER_PHONE_NO { get; set; }
        public string URUId { get; set; }
        public UserActivity FROM_SOURCE { get; set; }
        public List<string> USER_ROLE_LIST { get; set; }
        public string FIELD_NAME { get; set; }
        public string OLD_VALUE { get; set; }
        public string NEW_VALUE { get; set; }
    }
}