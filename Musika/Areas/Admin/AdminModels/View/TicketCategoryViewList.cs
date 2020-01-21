using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Musika.Areas.Admin.AdminModels.View
{
    public class TicketCategoryViewList
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string CategoryPrice { get; set; }
        public string RecordStatus { get; set; }
        public int PageNo { get; set; }
        public int T_Pages { get; set; }
    }
}