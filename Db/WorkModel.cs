using System.ComponentModel.DataAnnotations;

namespace BlazorServerApp.Db;

public class WorkModel {
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty; 
    public DateTime? DateCreation { get; set; } 
    public DateTime? DateDeliveryExpected { get; set; }
    public bool IsDelete { get; set; }
}