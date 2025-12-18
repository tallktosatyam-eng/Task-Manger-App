using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManager.Data;
using TaskManager.Models;

namespace TaskManager.Controllers
{
    [Authorize]
    public class ProjectsController : Controller
    {
        private readonly ApplicationDbContext _db;
        public ProjectsController(ApplicationDbContext db)
        {
            _db = db;
        }

        // GET: List All Projects
        public IActionResult Index()
        {
            ViewBag.TotalProjects = _db.Projects.Count();
            var list = _db.Projects.ToList();
            return View(list);
        }


        // GET: Create Page
        [Authorize(Roles = "Admin,Manager")]
        public IActionResult Create() => View();

        // POST: Create New Project
        [HttpPost, Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(Project model)
        {
            if (!ModelState.IsValid)
                return View(model);

            _db.Projects.Add(model);
            await _db.SaveChangesAsync();

            TempData["Message"] = "Project created successfully!";
            return RedirectToAction("Index");
        }

        // GET: Edit Page
        [Authorize(Roles = "Admin")]
        public IActionResult Edit(int id)
        {
            var project = _db.Projects.Find(id);
            if (project == null)
                return NotFound();

            return View(project);
        }

        // POST: Update Project
        [HttpPost, Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(Project model)
        {
            if (!ModelState.IsValid)
                return View(model);

            _db.Projects.Update(model);
            await _db.SaveChangesAsync();

            TempData["Message"] = "Project updated successfully!";
            return RedirectToAction("Index");
        }

        // GET: Delete Confirmation Page
        [Authorize(Roles = "Admin")]
        public IActionResult Delete(int id)
        {
            var project = _db.Projects.Find(id);
            if (project == null)
                return NotFound();

            return View(project);
        }

        // POST: Delete Project from DB
        [HttpPost, ActionName("Delete"), Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var project = _db.Projects.Find(id);
            if (project == null)
                return NotFound();

            _db.Projects.Remove(project);
            await _db.SaveChangesAsync();

            TempData["Message"] = "Project deleted successfully!";
            return RedirectToAction("Index");
        }
    }
}


