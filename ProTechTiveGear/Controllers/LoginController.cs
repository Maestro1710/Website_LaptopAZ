using ProTechTiveGear.Models;
using ProTechTiveGear.Services;
using System.Linq;
using System.Web.Mvc;

namespace ProTechTiveGear.Controllers
{
    public class LoginController : Controller
    {
        private readonly ProTechTiveGearEntities db = new ProTechTiveGearEntities();
        private readonly AuthService authService;

        public LoginController()
        {
            authService = new AuthService(new ProTechTiveGearEntities());
        }

        // GET: Login
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(FormCollection collection)
        {
            var userName = collection["userName"];
            var passWord = collection["passWord"];

            var cs = authService.Login(userName, passWord);
            if (cs != null)
            {
                Session["usr"] = cs;
                return RedirectToAction("Index", "AuraStore");
            }
            ModelState.AddModelError("", "Tài khoản hoặc mật khẩu không đúng");
            return View();
        }

        public ActionResult Register() => View();

        [HttpPost]
        public ActionResult Register(FormCollection collection)
        {
            string userName = collection["Username"];
            string passWord = collection["Password"];
            string confirmPassWord = collection["ConfirmPassword"];
            string name = collection["Name"];
            string email = collection["Email"];
            string address = collection["Address"];
            string phone = collection["PhoneNumber"];

            if (passWord == confirmPassWord)
            {
                var cs = new Customer
                {
                    Username = userName,
                    Passwords = passWord,
                    Name = name,
                    EmailAddress = email,
                    Address = address,
                    Phone = phone
                };

                if (authService.Register(cs))
                {
                    return RedirectToAction("Login");
                }
                ModelState.AddModelError("", "Tài khoản đã tồn tại!");
            }
            else
            {
                ViewBag.Confirm = "Mật khẩu không trùng khớp";
            }
            return View();
        }

        public ActionResult Changepassword()
        {
            if (Session["usr"] == null) return RedirectToAction("Login");

            var ac = (Customer)Session["usr"];
            return View(new AccountClientEntity(ac));
        }

        [HttpPost]
        public ActionResult Changepassword(FormCollection fc)
        {
            if (Session["usr"] == null) return RedirectToAction("Login");

            string userName = fc["userName"];
            string pass = fc["pass"];
            string newPass = fc["newpass"];
            string rePass = fc["repass"];

            if (newPass == rePass && authService.ChangePassword(userName, pass, newPass))
            {
                Session["usr"] = db.Customers.SingleOrDefault(x => x.Username == userName);
                return RedirectToAction("Profile", "AuraStore");
            }

            ModelState.AddModelError("", "Không thể thay đổi mật khẩu");
            var ac = (Customer)Session["usr"];
            return View(new AccountClientEntity(ac));
        }
    }
}
