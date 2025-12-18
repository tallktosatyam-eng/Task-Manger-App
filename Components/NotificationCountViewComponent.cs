using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TaskManager.Data;

namespace TaskManager.Components
{
    public class NotificationCountViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _db;

        public NotificationCountViewComponent(ApplicationDbContext db)
        {
            _db = db;
        }

        public IViewComponentResult Invoke()
        {
            if (!User.Identity.IsAuthenticated)
                return Content(""); // no notification for guests

            var idValue = ((ClaimsPrincipal)User).FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(idValue, out int userId))
                return Content(""); // claim missing or not an integer

            int unreadCount = _db.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .Count();

            return View(unreadCount);
        }
    }
}
