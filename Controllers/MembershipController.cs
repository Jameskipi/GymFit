using GymFit.Data;
using GymFit.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GymFit.Controllers
{
    public class MembershipController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MembershipController(ApplicationDbContext context)
        {
            _context = context;
        }

        // list
        public IActionResult Index()
        {
            var memberships = _context.MembershipOffers
                .Where(x => x.IsActive)
                .ToList();

            return View(memberships);
        }

        // purchase
        [HttpPost]
        public IActionResult Buy(int id)
        {
            var email = HttpContext.Session.GetString("UserEmail");

            if (email == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var user = _context.Users
                .FirstOrDefault(x => x.Email == email);

            var offer = _context.MembershipOffers
                .FirstOrDefault(x => x.Id == id);

            if (user == null || offer == null)
            {
                return RedirectToAction("Index");
            }

            var membership = new MembershipClient
            {
                UserId = user.Id,
                MembershipOfferId = offer.Id,
                PurchaseDate = DateTime.Now,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(offer.ValidityDays),
                PaymentStatus = PaymentStatus.Paid
            };

            _context.ClientMemberships.Add(membership);
            _context.SaveChanges();

            TempData["Success"] = "Membership purchased successfully!";

            return RedirectToAction("Index");
        }

        // bought memberships
        public IActionResult MyMemberships()
        {
            var email = HttpContext.Session.GetString("UserEmail");

            if (email == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var user = _context.Users
                .FirstOrDefault(x => x.Email == email);

            var memberships = _context.ClientMemberships
                .Include(x => x.MembershipOffer)
                .Where(x => x.UserId == user.Id)
                .OrderByDescending(x => x.PurchaseDate)
                .ToList();

            return View(memberships);
        }
    }
}