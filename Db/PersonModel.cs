using System.ComponentModel.DataAnnotations;

namespace BlazorServerApp.Db;

public class PersonModel
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Nom { get; set; }

    [Required, MaxLength(100)]
    public string Prenom { get; set; }

    [MaxLength(100)]
    public string? Service { get; set; }

    [MaxLength(100)]
    public string? Qualification { get; set; }
}