using OnlineShoping.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OnlineShoping.Models
{
    public class item
    {
        public Tbl_Product Product { get; set; }
        public int Quantity { get; set; }

    }
}