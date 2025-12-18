using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskManager.Models
{
    public class TaskItem
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Task title is required")]
        public string Title { get; set; } = "";

        public string? Description { get; set; }

        // Foreign key → User
        [Display(Name = "Assigned To")]
        public int? AssignedToUserId { get; set; }

        [ForeignKey(nameof(AssignedToUserId))]
        public User? AssignedTo { get; set; }

        // Foreign key of Project
        [Required(ErrorMessage = "Project must be selected")]
        public int ProjectId { get; set; }

        [ForeignKey(nameof(ProjectId))]
        public Project? Project { get; set; }

        [Required(ErrorMessage = "Task status is required")]
        public string Status { get; set; } = "Not Started";

        [Required(ErrorMessage = "Priority is required")]
        public string Priority { get; set; } = "Medium";

        [Display(Name = "Deadline")]
        public DateTime? Deadline { get; set; }

        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Display(Name = "Progress")]
        public int Progress { get; set; } = 0;
    }
}
