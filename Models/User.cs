using System.ComponentModel.DataAnnotations;

namespace TaskManager.Models
{
    public class User
    {
        public int Id { get; set; }
        [Required] public string Name { get; set; } = "";
        [Required][EmailAddress] public string Email { get; set; } = "";
        [Required] public string PasswordHash { get; set; } = "";
        public int RoleId { get; set; }
        public Role? Role { get; set; }
        public ICollection<TaskItem>? AssignedTasks { get; set; }
        public ICollection<Notification>? Notifications { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public double Rating { get; set; } = 0;  // average rating given by manager
        public int RatingCount { get; set; } = 0;

    }
}
