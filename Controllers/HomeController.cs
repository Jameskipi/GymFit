using GymFit.Data;
using GymFit.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QRCoder;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using static QRCoder.PayloadGenerator;

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
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Dashboard");
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(string email, string password)
        {
            string hash = Hash(password);

            var user = _context.Users
                .FirstOrDefault(x => x.Email == email && x.PasswordHash == hash);

            if (user == null)
            {
                ViewBag.Error = "Invalid login";
                return View();
            }

            if (user.IsBlocked)
            {
                ViewBag.Error = "Your account has been blocked";
                return View();
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

            return RedirectToAction("Dashboard");
        }

        // register
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(string email, string password, string confirmPassword, string firstName, string lastName)
        {
            if (_context.Users.Any(x => x.Email == email))
            {
                ViewBag.Error = "Email already exists";
                ViewBag.FirstName = firstName;
                ViewBag.LastName = lastName;
                return View();
            }

            if (password != confirmPassword)
            {
                ViewBag.Error = "Passwords do not match";
                ViewBag.FirstName = firstName;
                ViewBag.LastName = lastName;
                ViewBag.Email = email;
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

        // Dashboard
        public IActionResult Dashboard()
        {
            var user = _context.Users
                .FirstOrDefault(x => x.Email == User.Identity.Name);

            if (user != null && user.IsBlocked)
            {
                return RedirectToAction("Logout");
            }

            return View();
        }

        [Authorize]
        public IActionResult GenerateQRCode()
        {
            string email = User.Identity.Name;

            bool activeMembership = CheckMembership(email);

            if (!activeMembership)
            {
                return Content("QR GENERATION ERROR: Make sure your membership is active");
            }

            string QRData = $"GYMFIT-{email}-{DateTime.Now:yyyyMMdd}";

            using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
            {
                using (QRCodeData qrCodeData = qrGenerator.CreateQrCode(QRData, QRCodeGenerator.ECCLevel.Q))
                {
                    using (PngByteQRCode qrCode = new PngByteQRCode(qrCodeData))
                    {
                        byte[] qrCodeAsPngByteArr = qrCode.GetGraphic(20);

                        return File(qrCodeAsPngByteArr, "image/png");
                    }
                }
            }
        }

        private bool CheckMembership(string email)
        {
            var user = _context.Users.FirstOrDefault(x => x.Email == email);

            if (user == null)
            {
                return false;
            }

            bool isActive = _context.ClientMemberships
                .Any(x => x.UserId == user.Id &&
                          x.EndDate >= DateTime.Now &&
                          x.PaymentStatus == PaymentStatus.Paid);

            return isActive;
        }

        // logout
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
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