namespace BlazorServerApp.Db;

public class EventModel {
    public int Id { get; set; }
    public WorkModel? Work { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty; 
    public DateTime DateCreation { get; set; } 
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public List<PersonModel> Participants { get; set; } = new List<PersonModel>();
    public bool IsDelete { get; set; }
}