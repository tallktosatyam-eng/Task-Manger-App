
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TaskManager.Data;
using TaskManager.Models;
using System.Security.Claims;

namespace TaskManager.Controllers
{
    [Authorize]
    public class TasksController : Controller
    {
        private readonly ApplicationDbContext _db;

        public TasksController(ApplicationDbContext db)
        {
            _db = db;
        }

        // ---------------- INDEX ----------------
        public async Task<IActionResult> Index()
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

            IQueryable<TaskItem> tasks = _db.TaskItems
                  .Include(t => t.Project)
                  .Include(t => t.AssignedTo);

            if (!User.IsInRole("Admin") && !User.IsInRole("Manager"))
                tasks = tasks.Where(t => t.AssignedToUserId == userId);

            return View(await tasks.ToListAsync());
        }

        // ---------------- CREATE ----------------
        [Authorize(Roles = "Admin,Manager")]
        public IActionResult Create()
        {
            ViewBag.Projects = new SelectList(_db.Projects, "Id", "Name");
            ViewBag.Users = new SelectList(_db.Users, "Id", "Name");
            return View();
        }

        [HttpPost, Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(TaskItem model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Projects = new SelectList(_db.Projects, "Id", "Name");
                ViewBag.Users = new SelectList(_db.Users, "Id", "Name");
                return View(model);
            }

            model.CreatedAt = DateTime.UtcNow;
            model.Status ??= "Not Started";

            _db.TaskItems.Add(model);
            await _db.SaveChangesAsync();

            // Notification for assignment
            if (model.AssignedToUserId.HasValue)
            {
                _db.Notifications.Add(new Notification
                {
                    UserId = model.AssignedToUserId.Value,
                    TaskItemId = model.Id,
                    Message = $"New task assigned: {model.Title}",
                    CreatedAt = DateTime.UtcNow
                });
                await _db.SaveChangesAsync();
            }

            TempData["Message"] = "Task created successfully!";
            return RedirectToAction("Index");
        }

        // ---------------- EDIT ----------------
        public async Task<IActionResult> Edit(int id)
        {
            var task = await _db.TaskItems.FindAsync(id);
            if (task == null)
                return NotFound();

            ViewBag.Projects = new SelectList(_db.Projects, "Id", "Name", task.ProjectId);
            ViewBag.Users = new SelectList(_db.Users, "Id", "Name", task.AssignedToUserId);

            return View(task);
        }

        [HttpPost, Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(TaskItem model)
        {
            var oldTask = await _db.TaskItems.AsNoTracking().FirstOrDefaultAsync(t => t.Id == model.Id);

            _db.TaskItems.Update(model);
            await _db.SaveChangesAsync();

            // notification if status or user changed
            if (model.AssignedToUserId.HasValue)
            {
                if (oldTask.Status != model.Status || oldTask.AssignedToUserId != model.AssignedToUserId)
                {
                    _db.Notifications.Add(new Notification
                    {
                        UserId = model.AssignedToUserId.Value,
                        TaskItemId = model.Id,
                        Message = $"Task updated: {model.Title} | Status: {model.Status}",
                        CreatedAt = DateTime.UtcNow
                    });
                    await _db.SaveChangesAsync();
                }
            }

            TempData["Message"] = "Task updated successfully!";
            return RedirectToAction("Index");
        }

        // ---------------- INLINE STATUS UPDATE (AJAX) ----------------
        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            var task = await _db.TaskItems.FindAsync(id);
            if (task == null)
                return Json(new { success = false, message = "Task not found" });

            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

            if (!User.IsInRole("Admin") && task.AssignedToUserId != userId)
                return Json(new { success = false, message = "Not allowed" });

            task.Status = status;
            await _db.SaveChangesAsync();

            return Json(new { success = true, newStatus = status });
        }

        // ---------------- INLINE PROGRESS UPDATE (AJAX) ----------------
        [HttpPost]
        public async Task<IActionResult> UpdateProgress(int id, int progress)
        {
            var task = await _db.TaskItems.FindAsync(id);
            if (task == null)
                return Json(new { success = false });

            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

            if (!User.IsInRole("Admin") && task.AssignedToUserId != userId)
                return Json(new { success = false, message = "Not allowed" });

            task.Progress = progress;

            if (progress == 100)
                task.Status = "Completed";

            await _db.SaveChangesAsync();

            return Json(new { success = true, progress });
        }

        // ---------------- DELETE ----------------
        public IActionResult Delete(int id)
        {
            var task = _db.TaskItems
                .Include(t => t.Project)
                .Include(t => t.AssignedTo)
                .FirstOrDefault(t => t.Id == id);

            if (task == null)
                return NotFound();

            return View(task);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var task = await _db.TaskItems.FindAsync(id);
            if (task == null)
                return NotFound();

            _db.TaskItems.Remove(task);
            await _db.SaveChangesAsync();

            TempData["Message"] = "Task deleted successfully!";
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> TaskDetails(int id)
        {
            var task = await _db.TaskItems
                .Include(t => t.Project)
                .Include(t => t.AssignedTo)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (task == null)
                return NotFound();

            return View(task);
        }


    }
}
