using GymFit.Data;
using GymFit.Models;
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
            var email = HttpContext.Session.GetString("UserEmail");

            if (email == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var user = _context.Users.FirstOrDefault(x => x.Email == email);

            if (user == null)
            {
                return RedirectToAction("Index", "Home");
            }

            return View(user);
        }

        // edit
        [HttpPost]
        public IActionResult Update(User model)
        {
            var email = HttpContext.Session.GetString("UserEmail");

            if (email == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var user = _context.Users.FirstOrDefault(x => x.Email == email);

            if (user == null)
            {
                return RedirectToAction("Index", "Home");
            }

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.Email = model.Email;

            _context.SaveChanges();

            // update session email
            HttpContext.Session.SetString("UserEmail", user.Email);

            ViewBag.Success = "Profile updated successfully";

            return View("Index", user);
        }
    }
}