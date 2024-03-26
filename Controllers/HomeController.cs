using DemoShoppingWebsite.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace DemoShoppingWebsite.Controllers
{
    public class HomeController : Controller
    {
        dbShoppingCarEntities db = new dbShoppingCarEntities();

        public ActionResult content()
        {
            return View();
        }
        public ActionResult Index()
        {
            var products = db.table_Product.OrderByDescending(m => m.Id).ToList();
            return View(products);
        }
    



    public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Register(table_Member Member)
        {
            //如果資料驗證未通過則回傳原本的View
            if (!ModelState.IsValid)
            {
                return View();
            }

            // 如果註冊的帳號不存在，才能執行新增與儲存
            var member = db.table_Member.Where(m => m.UserId == Member.UserId).FirstOrDefault();
            if (member == null)
            {
                db.table_Member.Add(Member);
                db.SaveChanges();

                return RedirectToAction("Login");
            }
            ViewBag.Message = "帳號已被使用，請重新註冊";
            return View();
        }

        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(string UserId, string Password)
        {
            //找出符合登入帳號與密碼的 Member資料
            var member = db.table_Member.Where(m => m.UserId == UserId && m.Password == Password).FirstOrDefault();
            if (member == null)
            {
                ViewBag.Message = "帳號or密碼錯誤，請重新確認登入";
                return View();
            }

            Session["Welcome"] = $"{member.Name} 您好";

            FormsAuthentication.RedirectFromLoginPage(UserId, true);

            return RedirectToAction("Index", "Member");
        }

        public ActionResult ShoppingCar()
        {
            string UserId = User.Identity.Name;

            var orderDetails = db.table_OrderDetail.Where(m => m.UserId == UserId && m.IsApproved == "否").ToList();
            return View(orderDetails);
        }

        public ActionResult AddCar(string ProductId)
        {
            //取得目前通過驗證的使用者名稱
            string userId = User.Identity.Name;

            //取得該使用者目前購物車內是否已有此商品，且尚未形成訂單的資料
            var currentCar = db.table_OrderDetail
                .Where(m => m.ProductId == ProductId && m.IsApproved == "否" && m.UserId == userId).FirstOrDefault();
            if (currentCar == null)
            {
                //如果篩選條件資料為null，代表要新增全新一筆訂單明細資料
                //將產品資料欄位一一對照至訂單明細的欄位
                var product = db.table_Product.Where(m => m.ProductId == ProductId).FirstOrDefault();
                var orderDetail = new table_OrderDetail();
                orderDetail.UserId = userId;
                orderDetail.ProductId = product.ProductId;
                orderDetail.Name = product.Name;
                orderDetail.Price = product.Price;
                orderDetail.Quantity = 1;
                orderDetail.IsApproved = "否";
                db.table_OrderDetail.Add(orderDetail);
            }
            else
            {
                //如果購物車已有此商品，僅需將數量加1
                currentCar.Quantity++;
            }

            //儲存資料庫並導至購物車檢視頁面
            db.SaveChanges();
            return RedirectToAction("ShoppingCar");
        }
    }

  
}