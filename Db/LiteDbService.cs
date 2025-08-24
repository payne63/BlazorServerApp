using LiteDB;

namespace BlazorServerApp.Db;

public class LiteDbService : IDisposable {
    
    private readonly LiteDatabase _db;
    private readonly ILiteCollection<PersonModel> _persons;
    private readonly ILiteCollection<WorkModel> _works;
    private readonly ILiteCollection<EventModel> _events;

    
    public LiteDbService() {
        var dbPath = Path.Combine("database", "LiteDb.db");
        Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);
        var mapper = BsonMapper.Global;
        mapper.EnumAsInteger = false;
        _db = new LiteDatabase(dbPath, mapper);
        _persons = _db.GetCollection<PersonModel>("persons");
        _works = _db.GetCollection<WorkModel>("works");
        _events = _db.GetCollection<EventModel>("events");
    }

    public List<PersonModel> GetAllPersonsActive()
        => _persons
            .Find(p => p.IsDelete != true)
            .OrderBy(p => p.Nom).ThenBy(p => p.Prenom)
            .ToList();
    
    public List<WorkModel> GetAllWorksActive()
        => _works
            .Find(p => p.IsDelete != true)
            .OrderBy(p => p.Name)
            .ToList();
    
    public List<EventModel> GetAllEventsActive()
        => _events
            .Find(p => p.IsDelete != true)
            .OrderBy(p => p.CustomerName)
            .ToList();
    
    
    public void Insert(PersonModel person) => _persons.Insert(person);
    public void Update(PersonModel person) => _persons.Update(person);
    public void SoftDelete(PersonModel person) => person.IsDelete = true;
    private void HardDelete(PersonModel person) => _persons.Delete(person.Id);
    
    public void Insert(WorkModel work) => _works.Insert(work);
    public void Update(WorkModel work) => _works.Update(work);
    public void SoftDelete(WorkModel work) => work.IsDelete = true;
    private void HardDelete( WorkModel work) => _works.Delete(work.Id);
    
    public void Insert(EventModel ev) => _events.Insert(ev);
    public void Update(EventModel ev) => _events.Update(ev);
    public void SoftDelete(EventModel ev) => ev.IsDelete = true;
    private void HardDelete(EventModel ev) => _events.Delete(ev.Id);
    
     
    public void Dispose() => _db.Dispose();
}