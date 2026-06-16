using GymFit.Data;
using GymFit.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        [Authorize]
        public async Task<IActionResult> Dashboard()
        {
            var user = _context.Users
                .FirstOrDefault(x => x.Email == User.Identity.Name);

            if (user != null && user.IsBlocked)
            {
                return RedirectToAction("Logout");
            }

            int userId = user?.Id ?? 0;

            DateTime now = DateTime.Now;

            var activities = await _context.GroupActivities
                .Include(a => a.Trainer).ThenInclude(t => t.User)
                .Include(a => a.Reservations)
                .Where(a => a.StartTime >= now)
                .OrderBy(a => a.StartTime)
                .AsNoTracking()
                .ToListAsync();

            ViewBag.CurrentUserId = userId;

            return View(activities);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignUpForActivity(int activityId)
        {
            var user = _context.Users.FirstOrDefault(x => x.Email == User.Identity.Name);
            if (user == null) return Challenge();

            bool hasPremiumMembership = _context.ClientMemberships
                .Any(x => x.UserId == user.Id &&
                          x.MembershipOffer.Name == "Premium" &&
                          x.StartDate <= DateTime.Now &&
                          x.EndDate >= DateTime.Now &&
                          x.PaymentStatus == PaymentStatus.Paid);

            if (!hasPremiumMembership)
            {
                TempData["Error"] = "You can only join activity when you have Premium membership.";
                return RedirectToAction(nameof(Dashboard));
            }
            // -------------------------------------------------------------------

            var activity = await _context.GroupActivities
                .Include(a => a.Reservations)
                .FirstOrDefaultAsync(a => a.Id == activityId);

            if (activity == null) return NotFound("Activity don't exist.");

            var alreadyBooked = activity.Reservations.Any(r => r.UserId == user.Id);
            if (alreadyBooked)
            {
                TempData["Error"] = "You already joined.";
                return RedirectToAction(nameof(Dashboard));
            }

            if (activity.Reservations.Count >= activity.CapacityLimit)
            {
                TempData["Error"] = "No more free spots.";
                return RedirectToAction(nameof(Dashboard));
            }

            var reservation = new GroupActivityReservation
            {
                UserId = user.Id,
                GroupActivityId = activityId,
                BookingDate = DateTime.Now,
                IsPresent = false
            };

            _context.GroupActivityReservations.Add(reservation);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Successfully joined: {activity.Name}!";
            return RedirectToAction(nameof(Dashboard));
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelReservation(int activityId)
        {
            var user = _context.Users.FirstOrDefault(x => x.Email == User.Identity.Name);
            if (user == null) return Challenge();

            var reservation = await _context.GroupActivityReservations
                .FirstOrDefaultAsync(r => r.GroupActivityId == activityId && r.UserId == user.Id);

            if (reservation == null)
            {
                TempData["Error"] = "We could not find your reservation.";
                return RedirectToAction(nameof(Dashboard));
            }

            _context.GroupActivityReservations.Remove(reservation);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Your reservation has been cancelled.";
            return RedirectToAction(nameof(Dashboard));
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