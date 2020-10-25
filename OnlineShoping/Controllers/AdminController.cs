using Newtonsoft.Json;
using OnlineShoping.DAL;
using OnlineShoping.Models;
using OnlineShoping.Repository;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OnlineShoping.Controllers
{
    public class AdminController : Controller
    {
        public GenericUnitofWork _unitOfWork = new GenericUnitofWork();
        
        private dbMyOnlineShoppingEntities _DBEntity = new dbMyOnlineShoppingEntities();
        // GET: Admin
        public ActionResult Dashboard()
        {
            return View();
        }

        public ActionResult Product()
        {
            List<Tbl_Product> AllProducts = _unitOfWork.GetRepositoryInstance<Tbl_Product>().GetAllRecordsIQueryable().Where(i => i.IsDelete == false).ToList();
            return View(AllProducts);
        }
        [HttpGet]
        public ActionResult UpdateProduct()
        {
         
            ViewBag.CategoryId = new SelectList(_DBEntity.Tbl_Category, "CategoryId", "CategoryName");
            return View();
        }
        [HttpPost]
        public ActionResult UpdateProduct(Tbl_Product Product)
        {
                 Product.IsDelete = false;
                Product.CreatedDate = DateTime.Now;
                _DBEntity.Tbl_Product.Add(Product);
                _DBEntity.SaveChanges();
                
       
                ViewBag.CategoryId = new SelectList(_DBEntity.Tbl_Category, "CategoryId", "CategoryName", Product.ProductId);
            return RedirectToAction("Product");
        }

        public ActionResult ProductEdit(int Productid)
        {
            return View( _unitOfWork.GetRepositoryInstance<Tbl_Product>().GetFirstorDefault(Productid));
          
        }
        [HttpPost]
        public ActionResult ProductEdit(Tbl_Product Product)
        {
        
            Product.ModifiedDate = DateTime.Now;
                _unitOfWork.GetRepositoryInstance<Tbl_Product>().Update(Product);
            return RedirectToAction("Product");
        }

        public ActionResult DeleteProduct(int Productid)
        {
            return View(_unitOfWork.GetRepositoryInstance<Tbl_Product>().GetFirstorDefault(Productid));
            
        }

        [HttpPost, ActionName("DeleteProduct")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(Tbl_Product Product)
        {
            _unitOfWork.GetRepositoryInstance<Tbl_Product>().Remove(Product);
            return RedirectToAction("Product");
        }


        public ActionResult Categories()
        {
            List<Tbl_Category> AllCategories = _unitOfWork.GetRepositoryInstance<Tbl_Category>().GetAllRecordsIQueryable().Where(i => i.IsDelete == false).ToList();
            return View(AllCategories);        
        }
        //public ActionResult AddCategory()
        //{

        //    return UpdateCategory(0);
        //}
        //public ActionResult UpdateCategory( int categoryId)
        //{
        //    CategoryDetail category;
        //        if(categoryId != null)
        //    {
        //        category = JsonConvert.DeserializeObject<CategoryDetail>(JsonConvert.SerializeObject(_unitOfWork.GetRepositoryInstance<Tbl_Category>().GetFirstorDefault(categoryId)));
        //    }else
        //    {
        //        category = new CategoryDetail();

        //    }
        //    return View("UpdateCategory" , category);
        //}
        public ActionResult UpdateCategory()
        {
            return View();

        }

        [HttpPost]
        public ActionResult UpdateCategory(Tbl_Category category)
        {
            category.IsActive = true;
            category.IsDelete = false;
            
            var name = _DBEntity.Tbl_Category.Where(a => a.CategoryName == category.CategoryName).ToList();

            if(name.Count() >= 1 )
            {
                ViewBag.Message = " sorry , this Category is allready taken";
                return View();

               
            }
            else
            {
                _DBEntity.Tbl_Category.Add(category);
                _DBEntity.SaveChanges();
                return RedirectToAction("Categories");
            }
           
            //_unitOfWork.GetRepositoryInstance<Tbl_Category>().Add(category);
          
        }

        public ActionResult CategoryEdit(int categoryid)
        {
            return View(_unitOfWork.GetRepositoryInstance<Tbl_Category>().GetFirstorDefault(categoryid));

        }

        [HttpPost]
        public ActionResult CategoryEdit(Tbl_Category category)
        {

            _unitOfWork.GetRepositoryInstance<Tbl_Category>().Update(category);
            return RedirectToAction("Categories");
        }

    


    }
}