using MY_WEBSITE_API.Models.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MY_WEBSITE_API.Models.Web.ConsumerTabletSetup
{
    public class OrderDetails
    {
        public string ORDER_DETAILS_ID { get; set; }
        public string ORDER_ID { get; set; }
        public string PRODUCT_ID { get; set; }
        public int QUANTITY { get; set; }
        public decimal UNIT_PRICE { get; set; }
        public decimal TOTAL_PRICE { get; set; }
        public string ORDER_DETAILS_ACTIVE_FLAG { get; set; }
        public string ORDER_DETAILS_DELETE_FLAG { get; set; }
        public DateTime UPDATE_DATETIME { get; set; }
        public string UPDATE_ID { get; set; }
        public UserActivity FROM_SOURCE { get; set; }
    }
}