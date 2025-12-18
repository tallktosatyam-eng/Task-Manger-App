using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TaskManager.Data;

namespace TaskManager.Controllers
{
    [Authorize]
    public class NotificationsController : Controller
    {
        private readonly ApplicationDbContext _db;
        public NotificationsController(ApplicationDbContext db)
        {

            _db = db;

        }

        public async Task<IActionResult> Index()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var list = await _db.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
            return View(list);
        }

        [HttpPost]
        public async Task<IActionResult> MarkRead(int id)
        {
            var n = await _db.Notifications.FindAsync(id);
            if (n != null) { n.IsRead = true; await _db.SaveChangesAsync(); }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> MarkReadQuick(int id)
        {
            var n = await _db.Notifications.FindAsync(id);

            if (n != null)
            {
                n.IsRead = true;
                await _db.SaveChangesAsync();
            }

            // Redirect back to notifications list
            // return RedirectToAction("Index");
            return Redirect("/Tasks");
        }

    }
}
