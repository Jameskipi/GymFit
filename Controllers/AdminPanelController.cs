using GymFit.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GymFit.Models;

namespace GymFit.Controllers
{
    public class AdminPanelController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminPanelController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var users = _context.Users
                .Include(x => x.PurchasedMemberships)
                .ThenInclude(x => x.MembershipOffer)
                .ToList();

            ViewBag.Memberships = _context.MembershipOffers
                .OrderBy(x => x.Price)
                .ToList();

            var purchasedMemberships = _context.ClientMemberships
                .Include(x => x.MembershipOffer)
                .ToList();

            var totalPurchased = purchasedMemberships.Count;

            var activeMemberships = purchasedMemberships.Count(x =>
                x.PaymentStatus == PaymentStatus.Paid &&
                x.EndDate >= DateTime.Now);

            var inactiveMemberships = purchasedMemberships.Count(x =>
                x.PaymentStatus != PaymentStatus.Paid ||
                x.EndDate < DateTime.Now);

            var revenue = purchasedMemberships
                .Where(x => x.PaymentStatus == PaymentStatus.Paid)
                .Sum(x => x.MembershipOffer.Price);

            var revenuePerMembership = _context.ClientMemberships
                .Include(x => x.MembershipOffer)
                .Where(x => x.PaymentStatus == PaymentStatus.Paid)
                .GroupBy(x => x.MembershipOffer.Name)
                .Select(g => new
                {
                    Name = g.Key,
                    Revenue = g.Sum(x => x.MembershipOffer.Price)
                })
                .ToList();

            ViewBag.RevenueChart = revenuePerMembership;
            ViewBag.TotalPurchased = totalPurchased;
            ViewBag.ActiveMemberships = activeMemberships;
            ViewBag.InactiveMemberships = inactiveMemberships;
            ViewBag.Revenue = revenue;

            return View(users);
        }

        // delete user
        [HttpPost]
        public IActionResult DeleteUser(int id)
        {
            var user = _context.Users.FirstOrDefault(x => x.Id == id);

            if (user != null)
            {
                _context.Users.Remove(user);
                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        // block/unblock user
        [HttpPost]
        public IActionResult ToggleBlock(int id)
        {
            var user = _context.Users.FirstOrDefault(x => x.Id == id);

            if (user != null)
            {
                user.IsBlocked = !user.IsBlocked;
                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        // change user role
        [HttpPost]
        public IActionResult ChangeRole(int id, UserRole role)
        {
            var user = _context.Users.FirstOrDefault(x => x.Id == id);

            if (user != null)
            {
                user.Role = role;
                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }
    }
}