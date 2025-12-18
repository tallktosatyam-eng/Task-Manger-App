using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManager.Data;

namespace TaskManager.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _db;
        public DashboardController(ApplicationDbContext db)
        {

            _db = db;
        }



        public async Task<IActionResult> Index()
        {
            ViewBag.TotalProjects = await _db.Projects.CountAsync();
            var tasks = await _db.TaskItems.Include(t => t.Project).Include(t => t.AssignedTo).ToListAsync();
            var total = tasks.Count;
            var completed = tasks.Count(t => t.Status == "Completed");
            var overdue = tasks.Count(t => t.Deadline.HasValue && t.Deadline.Value < DateTime.UtcNow && t.Status != "Completed");
            var upcoming = tasks.Where(t => t.Deadline.HasValue && t.Deadline.Value >= DateTime.UtcNow).OrderBy(t => t.Deadline).Take(10).ToList();
            var InProgress = tasks.Count(t => t.Status == "In Progress");
            var NotStarted = tasks.Count(t => t.Status == "Not Started");
            // var TotalProjects = await _db.Projects.CountAsync();
            // var NotCompletedProjects = await _db.Projects
            //     .Where(p => _db.TaskItems.Any(t => t.ProjectId == p.Id && t.Status != "Completed"))
            //     .CountAsync();

            var OverDue1 = tasks.Count(t => t.Deadline.HasValue && t.Deadline.Value < DateTime.UtcNow && t.Status != "Completed");

            ViewBag.Total = total;
            ViewBag.Completed = completed;
            ViewBag.Overdue = overdue;
            ViewBag.Upcoming = upcoming;
            ViewBag.InProgress = InProgress;
            ViewBag.NotStarted = NotStarted;
            ViewBag.Overdue1 = OverDue1;
            return View();
        }

        public IActionResult CompletedTask()
        {
            var completedTask = _db.TaskItems
                .Include(t => t.AssignedTo)
                .Include(t => t.Project)
                .Where(t => t.Status == "Completed")
                .ToList();

            return View(completedTask);
        }

        public IActionResult OverdueTask()
        {
            var overdueTask = _db.TaskItems
                .Include(t => t.AssignedTo)
                .Include(t => t.Project)
                .Where(t => t.Deadline.HasValue && t.Deadline.Value < DateTime.UtcNow && t.Status != "Completed")
                .ToList();

            return View(overdueTask);
        }

    }
}
