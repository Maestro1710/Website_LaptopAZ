using ProTechTiveGear.Models;
using ProTechTiveGear.Services;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using ClosedXML.Excel;
using System;

namespace ProTechTiveGear.Controllers
{
    public class AdminController : Controller
    {
        private readonly AdminService adminService;

        public AdminController()
        {
            adminService = new AdminService(new ProTechTiveGearEntities());
        }

        public ActionResult Login() => View();

        [HttpPost]
        public ActionResult Login(FormCollection collection)
        {
            var username = collection["userName"];
            var password = collection["passWord"];

            var admin = adminService.Login(username, password);
            if (admin != null)
            {
                Session["Account"] = admin;
                Response.Cookies["usr"].Value = admin.Username;
                Response.Cookies["Name"].Value = admin.Name;
                Response.Cookies["avatar"].Value = string.IsNullOrEmpty(admin.Picture)
                                                    ? "~/img/Item/avatar-default-icon.png"
                                                    : admin.Picture;

                return RedirectToAction("Index", "Items");
            }

            ModelState.AddModelError("", "The user login or password is incorrect.");
            return View();
        }

        public ActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Admin admin)
        {
            if (ModelState.IsValid && adminService.CreateAdmin(admin))
                return RedirectToAction("Index", "Items");

            ModelState.AddModelError("", "Admin account already exists.");
            return View(admin);
        }

        public ActionResult ListOrder()
        {
            var orders = adminService.GetOrders().ToList();
            return View(orders);
        }

        public ActionResult Confirm(long? id)
        {
            if (id == null) return HttpNotFound();

            var details = adminService.GetOrderDetails(id.Value).ToList();
            ViewBag.Date = details.FirstOrDefault()?.Order.Orderdate;
            ViewBag.Status = details.FirstOrDefault()?.Order.Status;
            ViewBag.id = id;

            return View(details);
        }

        [HttpPost]
        public ActionResult Confirm(FormCollection fc)
        {
            long id = long.Parse(fc["id"]);
            int status = int.Parse(fc["status"]);

            if (adminService.ConfirmOrder(id, status))
                return RedirectToAction("ListOrder");

            ModelState.AddModelError("", "Cannot confirm order");
            return RedirectToAction("Confirm", new { id });
        }

        public ActionResult AllListOrder()
        {
            var orders = adminService.GetAllOrders().ToList();
            return View(orders);
        }

        public ActionResult SignOut()
        {
            Session.Clear();
            Response.Cookies.Clear();
            return RedirectToAction("Login", "Admin");
        }

        public ActionResult OrderDetail(long id)
        {
            var details = adminService.GetOrderDetails(id).ToList();

            if (details == null || !details.Any())
                return HttpNotFound();

            // Có thể gán thêm thông tin đơn hàng nếu cần hiển thị
            ViewBag.OrderId = id;
            ViewBag.OrderDate = details.FirstOrDefault()?.Order.Orderdate;
            ViewBag.CustomerName = details.FirstOrDefault()?.Order.Customer?.Name;

            return View(details);
        }

        public FileResult ExportSoldOrders(int month, int year)
        {
            var fileBytes = adminService.ExportSoldOrdersToExcel(month, year);
            string fileName = $"BaoCaoDonHang_{month:D2}_{year}.xlsx";

            return File(fileBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        }



    }
}
