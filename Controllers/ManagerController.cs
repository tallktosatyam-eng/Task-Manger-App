using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManager.Data;
using TaskManager.Models;

namespace TaskManager.Controllers
{
    [Authorize(Roles = "Manager")]
    public class ManagerController : Controller
    {
        private readonly ApplicationDbContext _db;

        public ManagerController(ApplicationDbContext db)
        {
            _db = db;
        }

        // ======================
        // MANAGER DASHBOARD
        // ======================
        public IActionResult Dashboard()
        {
            var tasks = _db.TaskItems
                .Include(t => t.Project)
                .Include(t => t.AssignedTo)
                .ToList();

            ViewBag.TotalTasks = tasks.Count;
            ViewBag.Completed = tasks.Count(t => t.Status == "Completed");
            ViewBag.Pending = tasks.Count(t => t.Status != "Completed");
            ViewBag.Overdue = tasks.Count(t =>
                t.Deadline < DateTime.UtcNow && t.Status != "Completed");

            return View(tasks);
        }

        // ======================
        // USER PERFORMANCE
        // ======================
        public IActionResult UserPerformance(int? userId, int? month)
        {
            // ViewBag.AllUsers = _db.Users.ToList();

            ViewBag.AllUsers = _db.Users
                                    .Include(u => u.Role)
                                    .Where(u => u.Role != null && u.Role.RoleName != "Admin" && u.Role.RoleName != "Manager")
                                    .ToList();


            if (userId == null)
                return View();

            var user = _db.Users.Find(userId);
            if (user == null)
                return View();

            var tasks = _db.TaskItems
                .Include(t => t.Project)
                .Where(t => t.AssignedToUserId == userId)
                .ToList();

            if (month.HasValue)
            {
                tasks = tasks
                    .Where(t => t.Deadline.HasValue &&
                                t.Deadline.Value.Month == month)
                    .ToList();
            }

            var projects = tasks
                .Where(t => t.Project != null)
                .Select(t => t.Project!)
                .DistinctBy(p => p.Id)
                .ToList();

            var ratings = _db.UserProjectRatings
                .Where(r => r.UserId == userId)
                .Include(r => r.Project)
                .OrderByDescending(r => r.RatedAt)
                .ToList();

            ViewBag.User = user;
            ViewBag.Tasks = tasks;
            ViewBag.UserProjects = projects;
            ViewBag.ProjectRatings = ratings;

            return View();
        }

        // ======================
        // RATE USER PER PROJECT
        // ======================
        [HttpPost]
        public IActionResult RateUserProject(int userId, int projectId, int rating)
        {
            var record = new UserProjectRating
            {
                UserId = userId,
                ProjectId = projectId,
                Rating = rating,
                RatedAt = DateTime.UtcNow
            };

            _db.UserProjectRatings.Add(record);
            _db.SaveChanges();

            TempData["Message"] = "Rating submitted successfully!";
            return RedirectToAction("UserPerformance", new { userId });
        }

        // ======================
        // MONTHLY REPORT
        // ======================
        public IActionResult MonthlyReport(int? month, int? year)
        {
            var tasks = _db.TaskItems
                .Include(t => t.Project)
                .Where(t => t.Deadline.HasValue)
                .AsQueryable();

            // Apply MONTH filter
            if (month.HasValue && month.Value > 0)
            {
                tasks = tasks.Where(t => t.Deadline!.Value.Month == month.Value);
            }

            // Apply YEAR filter
            if (year.HasValue && year.Value > 0)
            {
                tasks = tasks.Where(t => t.Deadline!.Value.Year == year.Value);
            }

            ViewBag.SelectedMonth = month;
            ViewBag.SelectedYear = year;

            return View(tasks.ToList());
        }

    }
}
