namespace BlazorServerApp.Db;

public class DbService
{
    public DbService(DataContext _dataContext)
    {
        GetDb = _dataContext;
    }

    public DataContext GetDb
    {
        get;
    }

    public IEnumerable<ContactModel> GetAllContacts() => GetDb.Contacts.AsEnumerable();

    public void Remove(ContactModel contact)
    {
        GetDb.Remove(contact);
        GetDb.SaveChanges();
    }

    public void Update(ContactModel contact)
    {
        GetDb.Update(contact);
        GetDb.SaveChanges();
    }

    public void Add(ContactModel contact)
    {
        GetDb.Add(contact);
        GetDb.SaveChanges();
    }

    public void Save() => GetDb.SaveChanges();
}