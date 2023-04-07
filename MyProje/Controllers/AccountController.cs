using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using MyProje.Entities;
using MyProje.Models;
using NETCore.Encrypt.Extensions;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace MyProje.Controllers
{
    public class AccountController : Controller
    {

        private readonly DatabaseContext _databaseContext;
        private readonly IConfiguration _configuration;


        public AccountController(DatabaseContext databaseContext, IConfiguration configuration)
        {
            _databaseContext = databaseContext;
            _configuration = configuration;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Login()
        {
            return View();
        }



        [HttpPost]
        public IActionResult Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                string hashedPassword = DoMd5HashedString(model.Password);

                User user = _databaseContext.Users.SingleOrDefault(x => x.UserName.ToLower() == model.Username.ToLower()
                && x.Password == hashedPassword);

                if (user != null)
                {
                    if (user.Locked)
                    {
                        ModelState.AddModelError(nameof(model.Username), "Username or password is incorrect");
                        return View(model);
                    }
                    List<Claim> claims = new List<Claim>();

                    //ClaimType. dedğimiz yerlere direk "id" veya "surname diyede verebilirsin"
                    claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()));
                    claims.Add(new Claim(ClaimTypes.Name, user.NameSurname ?? string.Empty));
                    claims.Add(new Claim(ClaimTypes.Role, user.Role));
                    claims.Add(new Claim("UserName", user.UserName));
                    // Demek "Cokies" yazmakda aynışey =>zaten üstüne geldiğinde cookie yazıyor                       
                    ClaimsIdentity identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme); ;
                    ClaimsPrincipal principal = new ClaimsPrincipal(identity);
                    //şimdi sign-in yapacağız
                    HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);  //Budeğer login oldugu zaman true olur
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError("", "Username or password is incorrect");
                }

            }
            return View(model);
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {

                if (_databaseContext.Users.Any(x => x.UserName.ToLower() == model.Username.ToLower()))
                {
                    ModelState.AddModelError(nameof(model.Username), "This Username is already exists.");
                    return View(model);
                }
                string hashedPassword = DoMd5HashedString(model.Password);

                User user = new()
                {
                    UserName = model.Username,
                    Password = hashedPassword,
                };
                _databaseContext.Users.Add(user);
                int affectedRowCount = _databaseContext.SaveChanges();
                if (affectedRowCount == 0)
                {
                    ModelState.AddModelError("", "User can not be added");
                }
                else
                {
                    return RedirectToAction(nameof(Login));
                }
            }
            return View(model);
        }

        [Authorize]
        public IActionResult Profile()
        {
            return View();
        }



        private string DoMd5HashedString(string gelenVeri)
        {
            string md5Salt = _configuration.GetValue<string>("AppSetting:MD5Salt");
            string salted = gelenVeri + md5Salt;
            string hashed = salted.MD5();
            return hashed;
        }

        public IActionResult Logout()
        {
            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(Login));
        }
    }
}
