using OnlineShoping.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OnlineShoping.Models
{
    public class OrderViewMogel
    {

        public string MemberName { get; set; }
      
        public IEnumerable<Tbl_Orders> Items { get; set; }
    }
}