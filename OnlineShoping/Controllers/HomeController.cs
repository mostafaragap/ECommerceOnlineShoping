using OnlineShoping.Models;
using OnlineShoping.DAL;
using OnlineShoping.Models.Home;
using OnlineShoping.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace OnlineShoping.Controllers
{
    public class HomeController : Controller
    {
         
        dbMyOnlineShoppingEntities ctx = new dbMyOnlineShoppingEntities();
        public GenericUnitofWork _unitOfWork = new GenericUnitofWork();
        private dbMyOnlineShoppingEntities db = new dbMyOnlineShoppingEntities();
        


        public ActionResult Index(string search, int? page)
        {
           
            
            HomeIndexViewModel model = new HomeIndexViewModel();
            return View(model.CreatedModel(search, 4, page));
        }

     



        /// <summary>
        /// ////////////////////////////////////////////////
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        
        public ActionResult AddToCart(int productId, string url)
        {
            if(Session["UserId"]!=null)
            {
                var UserId = (int)Session["UserId"];


                var cart = _unitOfWork.GetRepositoryInstance<Tbl_Cart>().GetFirstOrDefaultByParameter(a => a.MemberId == UserId && a.ProductId == productId && a.OrderStatues == false && a.Confirmed == false);

                if (cart == null || cart.OrderStatues==true)
                {
                    cart = new Tbl_Cart()
                    {
                        ProductId = productId,
                        Quantity = 1,
                        MemberId = UserId,
                    
                        Confirmed = false , 
                        OrderStatues = false
                       
                    }
                    ;

                    _unitOfWork.GetRepositoryInstance<Tbl_Cart>().Add(cart);
                   


                }
                else
                {
                    int prvQty = (int)cart.Quantity;

                    cart.Quantity = prvQty + 1;
                    _unitOfWork.GetRepositoryInstance<Tbl_Cart>().Update(cart);

                }

            }
            else
            {
                return RedirectToAction("Login", "Account");
            }
            
            return Redirect(url);
        }

        public ActionResult DecreaseQty(int productId)
        {
            if (Session["UserId"] != null)
            {
                var UserId = (int)Session["UserId"];
                var cart = _unitOfWork.GetRepositoryInstance<Tbl_Cart>().GetFirstOrDefaultByParameter(a => a.MemberId == UserId && a.ProductId == productId && a.OrderStatues == false && a.Confirmed==false);

                if (cart != null && cart.Quantity != 1)
                {
                    int prvQty = (int)cart.Quantity;

                    cart.Quantity = prvQty - 1;
                    _unitOfWork.GetRepositoryInstance<Tbl_Cart>().Update(cart);
                }
                else if (cart != null && cart.Quantity <= 1)
                {
                    _unitOfWork.GetRepositoryInstance<Tbl_Cart>().Remove(cart);
                }

                //if (Session["cart"] != null)
                //{
                //    List<item> cart = (List<item>)Session["cart"];
                //    var product = ctx.Tbl_Product.Find(productId);
                //    foreach (var item in cart)
                //    {
                //        if (item.Product.ProductId == productId)
                //        {
                //            int prevQty = item.Quantity;
                //            if (prevQty > 0)
                //            {
                //                cart.Remove(item);
                //                cart.Add(new item()
                //                {
                //                    Product = product,
                //                    Quantity = prevQty - 1
                //                });
                //            }
                //            break;
                //        }
                //    }
                //    Session["cart"] = cart;
                //}
            }
            else
            {
                return RedirectToAction("Login", "Account");
            }
            return Redirect("Checkout");
        }


        public ActionResult RemoveFromCart(int productId)
        {
            if (Session["UserId"] != null)
            {
                var UserId = (int)Session["UserId"];

                Tbl_Cart cart = _unitOfWork.GetRepositoryInstance<Tbl_Cart>().GetFirstOrDefaultByParameter(a => a.MemberId == UserId && a.ProductId == productId && a.OrderStatues == false && a.Confirmed == false);
                _unitOfWork.GetRepositoryInstance<Tbl_Cart>().Remove(cart);
            }
            else
            {
                return RedirectToAction("Login", "Account");
            }
            return RedirectToAction("Index"); 

        }

        
        public ActionResult Checkout()
        {
            if (Session["UserId"] != null)
            {
                var id = (int)Session["UserId"];
                dbMyOnlineShoppingEntities _db = new dbMyOnlineShoppingEntities();
                var carts = _db.Tbl_Cart.Where(a => a.MemberId == id && a.OrderStatues == false && a.Confirmed == false).ToList();
                if (carts != null)
                {
                    return View(carts);
                }
                else
                {
                    return ViewBag.Carts = "No Items Added yet !";
                }
            }
            else
            {
                return RedirectToAction("Login", "Account");
            }

        }

        public ActionResult CheckoutDetails()
        {
            if (Session["UserId"] != null)
            {
                var id = (int)Session["UserId"];
                dbMyOnlineShoppingEntities _db = new dbMyOnlineShoppingEntities();
                var carts = _db.Tbl_Cart.Where(a => a.MemberId == id && a.OrderStatues == false && a.Confirmed == false).ToList();
                if (carts != null)
                {
                
                    return View(carts);
                  
                }
                else
                {
                    return ViewBag.Carts = "No Items Added yet !";
                }
            }else
            {
                return RedirectToAction("Login", "Account");
            }
        }
        [HttpPost]
        public ActionResult Logout()
        {
            if(Session["UserId"] != null)
            {
                Session["UserId"] = null;
                Session["UserName"] = null;
                Response.Cache.SetCacheability(HttpCacheability.NoCache);
                Response.Cache.SetExpires(DateTime.Now);
                return RedirectToAction("Index"); 
            }
            return RedirectToAction("Index");

        }

        public ActionResult LogoutAdmin()
        {
            if (Session["AdminId"] != null)
            {
                Session["AdminId"] = null;
                Session["AdminName"] = null;
                return RedirectToAction("Login", "Account");
            }
            return RedirectToAction("Login", "Account");
         

        }


        public ActionResult MyOrders()
        {
            if (Session["UserId"] != null)
            {
                var id = (int)Session["UserId"];
                dbMyOnlineShoppingEntities _db = new dbMyOnlineShoppingEntities();
                var carts = _db.Tbl_Cart.Where(a => a.MemberId == id && a.OrderStatues == true && a.Confirmed == false).ToList();
                if (carts != null)
                {
                    Session["Carts"] = carts;
                    return View(carts);

                }
                else
                {
                    return ViewBag.Carts = "No Items Ordered yet !";
                }
            }
            else
            {
                return RedirectToAction("Login", "Account");
            }
        }


        [HttpPost]
        public ActionResult Order (List<Tbl_Cart> carts)
        {
            if(carts !=null)
            {
                foreach(var item in carts)
                {
                    item.OrderStatues = true;
                    _unitOfWork.GetRepositoryInstance<Tbl_Cart>().Update(item);
                    

                }
                ViewBag.Orders = "Orders Are Sent To Seller Succesfully";
                return RedirectToAction("MyOrders");

            }
          
            return RedirectToAction("MyOrders");
        }

     



    }
}