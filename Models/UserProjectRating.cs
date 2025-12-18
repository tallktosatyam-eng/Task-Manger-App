namespace TaskManager.Models
{
    

public class UserProjectRating
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int ProjectId { get; set; }
    public int Rating { get; set; }
    public DateTime RatedAt { get; set; }

    public User? User { get; set; }
    public Project? Project { get; set; }
}
}
