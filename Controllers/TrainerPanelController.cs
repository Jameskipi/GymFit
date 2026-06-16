using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GymFit.Data;
using GymFit.Models;

namespace GymFit.Controllers
{
    [Authorize(Roles = "Trainer")]
    public class TrainerPanelController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TrainerPanelController(ApplicationDbContext context)
        {
            _context = context;
        }

        private int GetCurrentUserId()
        {
            string? userEmail = User.Identity?.Name;
            if (string.IsNullOrEmpty(userEmail)) return 0;

            var user = _context.Users.FirstOrDefault(u => u.Email == userEmail);
            return user?.Id ?? 0;
        }

        public async Task<IActionResult> ManageProfile()
        {
            int trainerId = GetCurrentUserId();

            var profile = await _context.TrainerProfiles
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.UserId == trainerId);

            if (profile == null)
            {
                var currentUser = await _context.Users.FindAsync(trainerId);

                if (currentUser == null)
                {
                    return NotFound("No user found.");
                }

                profile = new TrainerProfile
                {
                    User = currentUser,
                    Biography = string.Empty,
                    Specializations = string.Empty,
                    PhotoUrl = null
                };

                _context.TrainerProfiles.Add(profile);
                await _context.SaveChangesAsync();
            }

            return View(profile);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ManageProfile(TrainerProfile model, IFormFile? photoFile)
        {
            string? userEmail = User.Identity?.Name;
            if (string.IsNullOrEmpty(userEmail)) return Challenge();

            var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);
            if (currentUser == null) return NotFound();

            var profileInDb = await _context.TrainerProfiles.FirstOrDefaultAsync(t => t.UserId == currentUser.Id);
            if (profileInDb == null) return NotFound("No profile found.");

            ModelState.Remove("User");

            if (ModelState.IsValid)
            {
                profileInDb.Biography = model.Biography ?? string.Empty;
                profileInDb.Specializations = model.Specializations ?? string.Empty;

                if (photoFile != null && photoFile.Length > 0)
                {
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(photoFile.FileName);
                    var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");

                    if (!Directory.Exists(uploadDir))
                    {
                        Directory.CreateDirectory(uploadDir);
                    }

                    var filePath = Path.Combine(uploadDir, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await photoFile.CopyToAsync(stream);
                    }

                    profileInDb.PhotoUrl = "/uploads/" + fileName;
                }

                _context.Update(profileInDb);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Profile updated successfully.";
                return RedirectToAction(nameof(ManageProfile));
            }

            model.User = currentUser;
            return View(model);
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

            var activities = await _context.GroupActivities
                .Include(a => a.Trainer).ThenInclude(t => t.User)
                .Include(a => a.Reservations)
                .OrderBy(a => a.StartTime)
                .AsNoTracking()
                .ToListAsync();

            ViewBag.CurrentUserId = userId;

            return View(activities);
        }

        public IActionResult CreateActivity()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateActivity(GroupActivity activity)
        {
            int trainerId = GetCurrentUserId();
            activity.TrainerId = trainerId;

            ModelState.Remove("Trainer");

            if (ModelState.IsValid)
            {
                _context.Add(activity);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Activities have been successfully added to the schedule.";
                return RedirectToAction(nameof(Dashboard));
            }
            return View(activity);
        }

        public async Task<IActionResult> ActivityDetails(int id)
        {
            int trainerId = GetCurrentUserId();
            var activity = await _context.GroupActivities
                .Include(a => a.Reservations)
                    .ThenInclude(r => r.User)
                .FirstOrDefaultAsync(a => a.Id == id && a.TrainerId == trainerId);

            if (activity == null) return NotFound("No activities found or you don't have permission to view them.");

            return View(activity);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleAttendance(int reservationId, bool isPresent)
        {
            var reservation = await _context.GroupActivityReservations
                .Include(r => r.GroupActivity)
                .FirstOrDefaultAsync(r => r.Id == reservationId);

            if (reservation == null) return NotFound();

            if (reservation.GroupActivity.TrainerId != GetCurrentUserId()) return Forbid();

            reservation.IsPresent = isPresent;
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteActivity(int id)
        {
            int trainerId = GetCurrentUserId();

            var activity = await _context.GroupActivities
                .Include(a => a.Reservations)
                .FirstOrDefaultAsync(a => a.Id == id && a.TrainerId == trainerId);

            if (activity == null)
            {
                return NotFound("No activities found or you don't have permission to delete them.");
            }

            if (activity.Reservations.Any())
            {
                _context.GroupActivityReservations.RemoveRange(activity.Reservations);
            }

            _context.GroupActivities.Remove(activity);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Activities have been successfully removed from the schedule.";
            return RedirectToAction(nameof(Dashboard));
        }
    }
}