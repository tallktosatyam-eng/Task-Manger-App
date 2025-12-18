using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManager.Data;
using TaskManager.Models;
using Microsoft.AspNetCore.Identity;

namespace TaskManager.Controllers
{
    [Authorize(Roles = "Admin")] // Admin only
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IPasswordHasher<User> _hasher;

        public AdminController(ApplicationDbContext db, IPasswordHasher<User> hasher)
        {
            _db = db;
            _hasher = hasher;
        }

        // List all users
        public async Task<IActionResult> Users()
        {
            var users = await _db.Users.Include(u => u.Role).ToListAsync();
            return View(users);
        }

        // GET Create User form
        public IActionResult Create()
        {
            ViewBag.Roles = _db.Roles.ToList();
            return View();
        }

        // POST Create User
        [HttpPost]
        public async Task<IActionResult> Create(string name, string email, string password, int roleId)
        {
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                TempData["Error"] = "Please fill all fields";
                ViewBag.Roles = _db.Roles.ToList();
                return View();
            }

            // Check if user exists
            var checkEmail = await _db.Users.FirstOrDefaultAsync(x => x.Email == email);
            if (checkEmail != null)
            {
                TempData["Error"] = "Email already exists!";
                ViewBag.Roles = _db.Roles.ToList();
                return View();
            }

            var user = new User
            {
                Name = name,
                Email = email,
                RoleId = roleId
            };

            // Hash password
            user.PasswordHash = _hasher.HashPassword(user, password);

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            TempData["Message"] = $"User {name} created successfully!";
            return RedirectToAction("Users");
        }

        // GET Reset Password form
        public IActionResult ResetPassword(int id)
        {
            var user = _db.Users.Find(id);
            if (user == null) return NotFound();

            return View(user);
        }

        // POST Reset Password
        [HttpPost]
        public IActionResult ResetPassword(int id, string newPassword)
        {
            var user = _db.Users.Find(id);
            if (user == null) return NotFound();

            if (string.IsNullOrWhiteSpace(newPassword))
            {
                TempData["Error"] = "Password must not be empty!";
                return RedirectToAction("ResetPassword", new { id });
            }

            user.PasswordHash = _hasher.HashPassword(user, newPassword);
            _db.Users.Update(user);
            _db.SaveChanges();

            TempData["Message"] = "Password reset successfully!";
            return RedirectToAction("Users");
        }

        public IActionResult DeleteUser(int id)
        {
            var user = _db.Users.Find(id);
            // if (user == null) return NotFound();

            _db.Users.Remove(user);
            _db.SaveChanges();

            TempData["Message"] = "User deleted successfully!";
            return RedirectToAction("Users");
        }

        // GET Delete Page
        public IActionResult DeleteUserPage(int id)
        {
            var user = _db.Users.Find(id);
            if (user == null) return NotFound();

            return View(user);
        }

        // GET Edit User Role form
        public IActionResult EditUserRole(int id)
        {
            var user = _db.Users.Find(id);
            if (user == null)
                return NotFound();

            ViewBag.Roles = _db.Roles.ToList();
            return View(user);
        }

        [HttpPost]
        public IActionResult EditRole(int id, int newRoleId)
        {
            var user = _db.Users.Find(id);
            if (user == null) return NotFound();

            user.RoleId = newRoleId;
            _db.Users.Update(user);
            _db.SaveChanges();

            TempData["Message"] = "User Role Updated Successfully!";
            return RedirectToAction("Users");
        }


    }
}
