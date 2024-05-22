namespace BlazorServerApp.Db;

public class ContactModel
{
    public int Id
    {
        get;
        set;
    }

    public string? Name
    {
        get;
        set;
    }

    public string? Surname
    {
        get;
        set;
    }

    public string? Company
    {
        get;
        set;
    }

    public string? Phone1
    {
        get;
        set;
    }

    public string? Phone2
    {
        get;
        set;
    }

    public string? Mail
    {
        get;
        set;
    }

    public string? Job
    {
        get;
        set;
    }

    public bool? IsMale
    {
        get;
        set;
    }

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
            IsMale = IsMale
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
            IsMale = contact.IsMale
        };
}