using MY_WEBSITE_API.Models.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MY_WEBSITE_API.Models.Web.ConsumerTabletSetup
{
    public class Product
    {
        public string PRODUCT_ID { get; set; }
        public string PRODUCT_NAME { get; set; }
        public string PRODUCT_DESC { get; set; }
        public decimal UNIT_COST {  get; set; }
        public decimal UNIT_SELLING_PRICE { get; set; }
        public int QUANTITY {  get; set; }
        public string PRODUCT_ACTIVE_FLAG { get; set; }
        public string PRODUCT_DELETE_FLAG { get; set; }
        public DateTime UPDATE_DATETIME { get; set; }
        public string UPDATE_ID { get; set; }
        public string UPDATE_NAME { get; set; }
        public UserActivity FROM_SOURCE { get; set; }
        public bool DUPLICATE_PRODUCT_NAME { get; set; }
    }
}