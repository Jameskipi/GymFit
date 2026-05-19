using GymFit.Data;
using GymFit.Models;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;

namespace GymFit.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        // login
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Index(string email, string password)
        {
            string hash = Hash(password);

            var user = _context.Users
                .FirstOrDefault(x => x.Email == email && x.PasswordHash == hash);

            if (user == null)
            {
                ViewBag.Error = "Invalid login";
                return View();
            }

            // nw czy to tak
            HttpContext.Session.SetString("UserEmail", user.Email);
            HttpContext.Session.SetString("UserRole", user.Role.ToString());

            return RedirectToAction("Dashboard");
        }

        // register
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(string email, string password, string firstName, string lastName)
        {
            if (_context.Users.Any(x => x.Email == email))
            {
                ViewBag.Error = "Email already exists";
                return View();
            }

            var user = new User
            {
                Email = email,
                PasswordHash = Hash(password),
                FirstName = firstName,
                LastName = lastName,
                Role = UserRole.Client
            };

            _context.Users.Add(user);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        // dash
        public IActionResult Dashboard()
        {
            return View();
        }

        // logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }

        // hash
        private string Hash(string input)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
            return Convert.ToBase64String(bytes);
        }
    }
}