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
        CompagnyToUpper(contact);
        GetDb.Update(contact);
        GetDb.SaveChanges();
    }

    public void Add(ContactModel contact)
    {
        CompagnyToUpper(contact);
        GetDb.Add(contact);
        GetDb.SaveChanges();
    }

    public void Save() => GetDb.SaveChanges();

    private void CompagnyToUpper(ContactModel contact) => contact.Company= contact.Company == null?  contact.Company : contact.Company.ToUpper();
}