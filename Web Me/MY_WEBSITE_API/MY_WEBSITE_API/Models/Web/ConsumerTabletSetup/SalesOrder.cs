using MY_WEBSITE_API.Models.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MY_WEBSITE_API.Models.Web.ConsumerTabletSetup
{
    public class SalesOrder
    {
        public string SALES_ORDER_ID { get; set; }
        public string CUSTOMER_ID { get; set; }
        public string CUSTOMER_NAME { get; set; }
        public string PRODUCT_ID { get; set; }
        public string PRODUCT_NAME { get; set; }
        public int AVAILABLE_QUANTITY { get; set; }
        public int QUANTITY {  get; set; }
        public decimal UNIT_SELLING_PRICE {  get; set; }
        public DateTime ORDER_DATETIME { get; set; }
        public decimal TOTAL_AMOUNT { get; set; }
        public string SALES_ORDER_ACCEPT_FLAG {  get; set; }
        public string ORDER_STATUS { get; set; }
        public string SALES_ORDER_ACTIVE_FLAG { get; set; }
        public string SALES_ORDER_DELETE_FLAG { get; set; }
        public DateTime UPDATE_DATETIME { get; set; }
        public string UPDATE_ID { get; set; }
        public string UPDATE_NAME { get; set; }
        public UserActivity FROM_SOURCE { get; set; }
        public List<string> SALES_ORDER_LIST { get; set; }
        public List<OrderDetails> OrderDetails { get; set; }
        public string EMAIL { get; set; }
        public string ADDRESS { get; set; }
        public string COMPANY_NAME { get; set; }
        public string PHONE_NO { get; set; }
    }
}