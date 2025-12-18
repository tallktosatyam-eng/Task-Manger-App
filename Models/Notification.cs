namespace TaskManager.Models
{
    public class Notification
    {
        public int Id { get; set; }  //notification Id primary key
        public int UserId { get; set; } //foreign key to User table
        public User? User { get; set; } //navigation property to User
        public int? TaskItemId { get; set; }    //foreign key to TaskItem table
        public TaskItem? TaskItem { get; set; } //navigation property to TaskItem
        public string Message { get; set; } = "";   //notification message
        public bool IsRead { get; set; } = false; //flag to indicate if notification is read
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;  //timestamp of notification creation
    }
}
