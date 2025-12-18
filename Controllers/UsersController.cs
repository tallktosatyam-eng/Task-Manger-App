using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManager.Data;
using TaskManager.Models;
using Microsoft.AspNetCore.Identity;

namespace TaskManager.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IPasswordHasher<User> _hasher;

        public UsersController(ApplicationDbContext db, IPasswordHasher<User> hasher)
        {
            _db = db;
            _hasher = hasher;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _db.Users.Include(u => u.Role).ToListAsync();
            return View(users);
        }

        [HttpGet]
        public IActionResult ResetPassword(int id)
        {
            var user = _db.Users.Find(id);
            return View(user);
        }

        [HttpPost]
        public IActionResult ResetPassword(int id, string newPassword)
        {
            var user = _db.Users.Find(id); //find user by id

            if (user == null)
                return NotFound();

            user.PasswordHash = _hasher.HashPassword(user, newPassword);

            _db.Users.Update(user); //update user entity
            _db.SaveChanges(); //commit changes

            TempData["Message"] = "Password updated successfully";
            return RedirectToAction("Index");
        }
    }
}
