using OnlineShoping.Models;
using OnlineShoping.DAL;
using OnlineShoping.Models.Home;
using OnlineShoping.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;


namespace OnlineShoping.Controllers
{
    public class HomeController : Controller
    {
        dbMyOnlineShoppingEntities ctx = new dbMyOnlineShoppingEntities();
        public GenericUnitofWork _unitOfWork = new GenericUnitofWork();
        public ActionResult Index(string search, int? page)
        {
            HomeIndexViewModel model = new HomeIndexViewModel();
            return View(model.CreatedModel(search, 4, page));
        }

        public ActionResult AddToCart(int productId)
        {
            if(Session["cart"] == null)
            {
                List<item> cart = new List<item>();
                var product = ctx.Tbl_Product.Find(productId);
                
                cart.Add(new item()
                {
                    Product = product,
                    Quantity = 1

                });
                Session["cart"] = cart;
            }else
            {
                List<item> cart = (List<item>)Session["cart"];
                var product = ctx.Tbl_Product.Find(productId);
                foreach (var item in cart)
                {
                    if (item.Product.ProductId == productId)
                    {
                        int PrvQty = item.Quantity;
                        cart.Remove(item);
                        cart.Add(new item()
                        {
                            Product = product,
                            Quantity = PrvQty +1

                        });

                        break;
                    }
                    else
                    {
                        cart.Add(new item()
                        {
                            Product = product,
                            Quantity = 1

                        });
                        break;
                    }
                }

                Session["cart"] = cart;
            }
          

            return RedirectToAction("Index");
        }
    

        public ActionResult RemoveFromCart(int productId)
        {
            List<item> cart = (List<item>)Session["cart"];

            foreach(var item in cart)
            {
if(item.Product.ProductId == productId)
                {
                    cart.Remove(item);
                    break;
      
                }
            }

   
            Session["cart"] = cart;
            return RedirectToAction("Index");

        }
    }
}