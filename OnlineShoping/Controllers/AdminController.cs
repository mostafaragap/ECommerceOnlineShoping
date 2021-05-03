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
            if (Session["AdminId"] != null)
            {
                return View();
            }
            else
           return     RedirectToAction("Login", "Account");
            
        }

        public ActionResult Product()
        {
            if (Session["AdminId"] != null)
            {
                List<Tbl_Product> AllProducts = _unitOfWork.GetRepositoryInstance<Tbl_Product>().GetAllRecordsIQueryable().Where(i => i.IsDelete == false).ToList();
                return View(AllProducts);
            }
            else
                return RedirectToAction("Login", "Account");
        }
        [HttpGet]
        public ActionResult UpdateProduct()
        {
            if (Session["AdminId"] != null)
            {

                ViewBag.CategoryId = new SelectList(_DBEntity.Tbl_Category, "CategoryId", "CategoryName");
                return View();
            }else
                return RedirectToAction("Login", "Account");

        }
        [HttpPost]
        public ActionResult UpdateProduct(Tbl_Product Product , HttpPostedFileBase file)
        {
            if (Session["AdminId"] != null)
            {
                string pic = null;
                if (file != null)
                {
                    pic = System.IO.Path.GetFileName(file.FileName);
                    string path = System.IO.Path.Combine(Server.MapPath("~/ProductImgs/"), pic);
                    file.SaveAs(path);
                }
                Product.ProductImage = pic;
                Product.IsDelete = false;
                Product.CreatedDate = DateTime.Now;
                _DBEntity.Tbl_Product.Add(Product);
                _DBEntity.SaveChanges();


                ViewBag.CategoryId = new SelectList(_DBEntity.Tbl_Category, "CategoryId", "CategoryName", Product.ProductId);
                return RedirectToAction("Product");
            }else
                return RedirectToAction("Login", "Account");

        }

        public ActionResult ProductEdit(int Productid)
        {
            if (Session["AdminId"] != null) { 
            ViewBag.CategoryId = new SelectList(_DBEntity.Tbl_Category, "CategoryId", "CategoryName");
            return View(_unitOfWork.GetRepositoryInstance<Tbl_Product>().GetFirstorDefault(Productid));
        }else
                return RedirectToAction("Login", "Account");
    }
        [HttpPost]
        public ActionResult ProductEdit(Tbl_Product Product, HttpPostedFileBase file)
        {
            if (Session["AdminId"] != null)
            {
                string pic = null;
                if (file != null)
                {
                    pic = System.IO.Path.GetFileName(file.FileName);
                    string path = System.IO.Path.Combine(Server.MapPath("~/ProductImgs/"), pic);
                    file.SaveAs(path);
                    Product.ProductImage = pic;
                }
                else
                {
                    Product.ProductImage = Product.ProductImage;
                }

                Product.ModifiedDate = DateTime.Now;
                _unitOfWork.GetRepositoryInstance<Tbl_Product>().Update(Product);
                ViewBag.CategoryId = new SelectList(_DBEntity.Tbl_Category, "CategoryId", "CategoryName", Product.ProductId);
                return RedirectToAction("Product");
            }else
                return RedirectToAction("Login", "Account");

        }

        public ActionResult DeleteProduct(int Productid)
        {

            if (Session["AdminId"] != null)
            {
                return View(_unitOfWork.GetRepositoryInstance<Tbl_Product>().GetFirstorDefault(Productid));
            }else
                return RedirectToAction("Login", "Account");

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
            if (Session["AdminId"] != null)
            {
                List<Tbl_Category> AllCategories = _unitOfWork.GetRepositoryInstance<Tbl_Category>().GetAllRecordsIQueryable().Where(i => i.IsDelete == false).ToList();
                return View(AllCategories);
            }else
                return RedirectToAction("Login", "Account");

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
            if (Session["AdminId"] != null)
            {
                return View();
            }
            else
                return RedirectToAction("Login", "Account");


        }

        [HttpPost]
        public ActionResult UpdateCategory(Tbl_Category category)
        {
            if (Session["AdminId"] != null)
            {
                category.IsActive = true;
                category.IsDelete = false;

                var name = _DBEntity.Tbl_Category.Where(a => a.CategoryName == category.CategoryName).ToList();

                if (name.Count() >= 1)
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
            }else
                return RedirectToAction("Login", "Account");


            //_unitOfWork.GetRepositoryInstance<Tbl_Category>().Add(category);

        }

        public ActionResult CategoryEdit(int categoryid)
        {
            return View(_unitOfWork.GetRepositoryInstance<Tbl_Category>().GetFirstorDefault(categoryid));

        }

        [HttpPost]
        public ActionResult CategoryEdit(Tbl_Category category)
        {
            if (Session["AdminId"] != null)
            {

                _unitOfWork.GetRepositoryInstance<Tbl_Category>().Update(category);
                return RedirectToAction("Categories");
            }else
                return RedirectToAction("Login", "Account");


        }




        public ActionResult GetOrders()
        {



            var Jobs = from app in _DBEntity.Tbl_Orders
                       join job in _DBEntity.Tbl_Members
                       on app.MemberID equals job.MemberId
                       where app.OrderStatues == false 
                       select app;

            var groubed = from j in Jobs
                          group j by j.Tbl_Members.EmailId
                          into gr
                          select new OrderViewMogel
                          {
                             
                              MemberName = gr.Key,
                              Items = gr
                             
                             
                          };

            if (Jobs.Count() < 1)
            {
                ViewBag.Res = "There is no Orders Now";
            }
            else
            {
                ViewBag.Res = "Orders";
                return View(groubed.ToList());
            }
            return View(groubed.ToList());
        }




   
        public ActionResult ConfirmOrder(int carts)
        {
            if (carts==0)
            {
                return HttpNotFound();
            }
            else
            {
                var orders = _unitOfWork.GetRepositoryInstance<Tbl_Orders>().GetListParameter(a => a.MemberID == carts);
                foreach(var item in orders)
                {
                    item.OrderStatues = true;
                    _unitOfWork.GetRepositoryInstance<Tbl_Orders>().Update(item);
                    var pro = _unitOfWork.GetRepositoryInstance<Tbl_Product>().GetFirstorDefault(item.Tbl_Product.ProductId);
                    int prevQuty = (int)pro.Quantity; 

                    pro.Quantity = prevQuty - item.Quantity;
                    _unitOfWork.GetRepositoryInstance<Tbl_Product>().Update(pro);

                }
                //foreach (var item in carts)
                //{
                //    var order = _unitOfWork.GetRepositoryInstance<Tbl_Orders>().GetFirstorDefault(item);
                //    order.OrderStatues = true;
                //    _unitOfWork.GetRepositoryInstance<Tbl_Orders>().Update(order);


                //}
                return RedirectToAction("GetOrders");
            }

        }


        public ActionResult ViewOrders(string memberName)
        {

       var user= _unitOfWork.GetRepositoryInstance<Tbl_Members>().GetFirstOrDefaultByParameter(a => a.EmailId == memberName);

            var orders = _unitOfWork.GetRepositoryInstance<Tbl_Orders>().GetListParameter(a => a.MemberID == user.MemberId && a.OrderStatues== false); 


            return View(orders);




        }

        //[HttpPost, ActionName("ConfirmOrder")]
        //[ValidateAntiForgeryToken]
        //public ActionResult ConfirmOrders(List<int> cartsId)
        //{
        //    List<Tbl_Cart> carts = new List<Tbl_Cart>();
        //    if (cartsId != null)
        //    {


        //        foreach (var item in cartsId)
        //        {

        //            var res=  _DBEntity.Tbl_Cart.Find(item);

        //            carts.Add(res);

        //        }

        //        return RedirectToAction("GetOrders");

        //    }

        //    return RedirectToAction("GetOrders");
        //}






    }
}