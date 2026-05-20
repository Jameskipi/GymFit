using GymFit.Data;
using GymFit.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace GymFit.Controllers
{
    public class ProfileController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProfileController(ApplicationDbContext context)
        {
            _context = context;
        }

        // profile
        [HttpGet]
        public IActionResult Index()
        {
            var email = User.Identity.Name;

            var user = _context.Users.FirstOrDefault(x => x.Email == email);

            if (user == null)
            {
                return RedirectToAction("Index", "Home");
            }

            return View(user);
        }

        // edit
        [HttpPost]
        public async Task<IActionResult> Update(User model)
        {
            var currentEmail = User.Identity.Name;

            var user = _context.Users.FirstOrDefault(x => x.Email == currentEmail);

            if (user == null)
            {
                return RedirectToAction("Index", "Home");
            }

            bool emailChanged = user.Email.ToLower() != model.Email.ToLower();

            if (emailChanged && _context.Users.Any(x => x.Email == model.Email))
            {
                ViewBag.Error = "Ten adres e-mail jest już zajęty przez innego użytkownika.";
                return View("Index", user);
            }

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.Email = model.Email;

            _context.SaveChanges();

            if (emailChanged)
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

                return RedirectToAction("Index", "Home");
            }

            ViewBag.Success = "Profil został zaktualizowany pomyślnie.";
            return View("Index", user);
        }
    }
}