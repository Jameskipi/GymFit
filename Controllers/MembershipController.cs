using GymFit.Data;
using GymFit.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

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
            var email = User.Identity.Name;

            var user = _context.Users
                .FirstOrDefault(x => x.Email == email);

            var offer = _context.MembershipOffers
                .FirstOrDefault(x => x.Id == id);

            if (user == null || offer == null)
            {
                return RedirectToAction("Index");
            }

            // membership already active check
            var activeMembership = _context.ClientMemberships
                .Include(x => x.MembershipOffer)
                .FirstOrDefault(x =>
                    x.UserId == user.Id &&
                    x.EndDate >= DateTime.Now &&
                    x.PaymentStatus == PaymentStatus.Paid
                );

            if (activeMembership != null)
            {
                TempData["Error"] =
                    $"You already have: {activeMembership.MembershipOffer.Name} valid until {activeMembership.EndDate:yyyy-MM-dd}";
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
            var email = User.Identity.Name;

            var user = _context.Users
                .FirstOrDefault(x => x.Email == email);

            var memberships = _context.ClientMemberships
                .Include(x => x.MembershipOffer)
                .Where(x => x.UserId == user.Id)
                .OrderByDescending(x => x.PurchaseDate)
                .ToList();

            return View(memberships);
        }

        [Authorize(Roles = "Administrator")]
        [HttpPost]
        public IActionResult Create(MembershipOffer model)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("Index");
            }

            _context.MembershipOffers.Add(model);
            _context.SaveChanges();

            TempData["Success"] = "Membership created";

            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Administrator")]
        [HttpPost]
        public IActionResult Edit(MembershipOffer model)
        {
            var offer = _context.MembershipOffers
                .FirstOrDefault(x => x.Id == model.Id);

            if (offer == null)
            {
                return RedirectToAction("Index");
            }

            offer.Name = model.Name;
            offer.Price = model.Price;
            offer.ValidityDays = model.ValidityDays;
            offer.Description = model.Description;
            offer.IsActive = model.IsActive;

            _context.SaveChanges();

            TempData["Success"] = "Membership updated";

            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Administrator")]
        [HttpPost]
        public IActionResult Delete(int id)
        {
            var offer = _context.MembershipOffers
        .FirstOrDefault(x => x.Id == id);

            if (offer != null)
            {
                offer.IsActive = false;

                _context.SaveChanges();
            }

            TempData["Success"] = "Membership deactivated";

            return RedirectToAction("Index");
        }

        // toggle membership
        [Authorize(Roles = "Administrator")]
        [HttpPost]
        public IActionResult ToggleActive(int id)
        {
            var offer = _context.MembershipOffers
                .FirstOrDefault(x => x.Id == id);

            if (offer != null)
            {
                offer.IsActive = !offer.IsActive;
                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        // cancel membership
        [HttpPost]
        public IActionResult Cancel(int id)
        {
            var email = User.Identity.Name;

            var user = _context.Users
                .FirstOrDefault(x => x.Email == email);

            if (user == null)
            {
                return RedirectToAction("MyMemberships");
            }

            var membership = _context.ClientMemberships
                .FirstOrDefault(x => x.Id == id && x.UserId == user.Id);

            if (membership == null)
            {
                return RedirectToAction("MyMemberships");
            }

            // already cancelled
            if (membership.PaymentStatus == PaymentStatus.Cancelled)
            {
                return RedirectToAction("MyMemberships");
            }

            membership.PaymentStatus = PaymentStatus.Cancelled;

            _context.SaveChanges();

            TempData["Success"] = "Membership cancelled";

            return RedirectToAction("MyMemberships");
        }
    }
}