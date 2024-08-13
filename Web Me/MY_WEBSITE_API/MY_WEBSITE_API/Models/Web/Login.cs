using MY_WEBSITE_API.Models.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MY_WEBSITE_API.Models.Web
{
    public class Login
    {
        public string USER_ID { get; set; }
        public string USER_NAME { get; set; }
        public string PASSWORD { get; set; }
        public int LOGIN_FAIL_COUNT { get; set; }
        public int PASSWORD_AGE { get; set; }
        public string PASSWORD_REGEX { get; set; }
        public string ACCOUNT_LOCK_FLAG { get; set; }
        public string RESET_FLAG { get; set; }
        public DateTime RESET_DATETIME { get; set; }
        public string DEFAULT_PASSWORD { get; set; }
        public string VALID { get; set; }
        public string ERROR_MSG { get; set; }
        public UserActivity FROM_SOURCE { get; set; }
        public string ACTIVE_FLAG { get; set; }
        public int AUTO_LOGOUT_DURATION { get; set; }
    }
}