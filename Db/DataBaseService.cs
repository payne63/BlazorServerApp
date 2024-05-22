namespace BlazorServerApp.Db;

public class DataBaseService
{
    private readonly DataBaseContext _getDb;
    
    public DataBaseService(DataBaseContext dataBaseContext) => _getDb = dataBaseContext;

    public DataBaseContext GetDb => _getDb;

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