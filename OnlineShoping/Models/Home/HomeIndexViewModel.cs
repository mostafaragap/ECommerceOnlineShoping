using OnlineShoping.DAL;
using OnlineShoping.Repository;
using PagedList;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace OnlineShoping.Models.Home
{
    public class HomeIndexViewModel
    {
        public  GenericUnitofWork _unitOfWork = new GenericUnitofWork();
        dbMyOnlineShoppingEntities context = new dbMyOnlineShoppingEntities();
        public IPagedList<Tbl_Product> ListOfProducts { get; set; }
        public  HomeIndexViewModel CreatedModel(string search , int pageSize, int? page )
        {
            SqlParameter[] paremeter = new SqlParameter[]
            {
                new SqlParameter("@search" , search ??(object)DBNull.Value)
            };
            IPagedList<Tbl_Product> data = context.Database.SqlQuery<Tbl_Product>("GetBySearch @search" , paremeter).ToList().ToPagedList(page ?? 1,  pageSize);
            return new HomeIndexViewModel()
            {
                ListOfProducts = data 
            };
        }
    }
}