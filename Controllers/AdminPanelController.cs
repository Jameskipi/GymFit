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