using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MY_WEBSITE_API.Models.Web.ConsumerTabletSetup
{
    public class SalesChartData
    {
        public int OrderYear { get; set; }
        public int OrderMonth { get; set; }
        public int OrderDay { get; set; }
        public decimal TotalAmount { get; set; }
    }
}