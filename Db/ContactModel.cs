using System.ComponentModel.DataAnnotations;

namespace BlazorServerApp.Db;

public class ContactModel
{
    public int Id
    {
        get;
        set;
    }
    [MaxLength(100)]
    public string? Name
    {
        get;
        set;
    }
    [MaxLength(100)]
    public string? Surname
    {
        get;
        set;
    }
    [MaxLength(100)]
    public string? Company
    {
        get;
        set;
    }
    [MaxLength(100)]
    public string? Phone1
    {
        get;
        set;
    }
    [MaxLength(100)]
    public string? Phone2
    {
        get;
        set;
    }
    [MaxLength(100)]
    public string? Mail
    {
        get;
        set;
    }

    [MaxLength(100)]
    public string? Job
    {
        get;
        set;
    }

    public bool Ismale
    {
        get;
        set;
    } = true;

    public ContactModel Clone() =>
        new()
        {
            Id = Id,
            Name = new string(Name),
            Surname = new string(Surname),
            Company = Company,
            Phone1 = Phone1,
            Phone2 = Phone2,
            Mail = Mail,
            Job = Job,
            Ismale = Ismale
        };

    public ContactModel SetValFrom(ContactModel contact) =>
        new()
        {
            Id = contact.Id,
            Name = contact.Name,
            Surname = contact.Surname,
            Company = contact.Company,
            Phone1 = contact.Phone1,
            Phone2 = contact.Phone2,
            Mail = contact.Mail,
            Job = contact.Job,
            Ismale = contact.Ismale
        };
}