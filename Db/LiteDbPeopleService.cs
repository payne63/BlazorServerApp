using LiteDB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BlazorServerApp.Db;

public class LiteDbPeopleService : IDisposable
{
    private readonly LiteDatabase _db;
    private readonly ILiteCollection<PersonModel> _people;

    public LiteDbPeopleService()
    {
        var dbPath = Path.Combine("database", "people_litedb.db");
        Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);

        var mapper = BsonMapper.Global;
        // Store enums as strings for readability
        mapper.EnumAsInteger = false;

        _db = new LiteDatabase(dbPath, mapper);
        _people = _db.GetCollection<PersonModel>("people");
        _people.EnsureIndex(p => p.Id, true);
        _people.EnsureIndex(p => p.Nom);
        _people.EnsureIndex(p => p.Prenom);

        // Seed minimal data if empty
        if (_people.Count() == 0)
        {
            _people.Insert(new PersonModel
            {
                Nom = "Dupont",
                Prenom = "Jean",
                Mail = "jean.dupont@example.com",
                Service = ServiceType.BureauDEtude,
                Qualification = QualificationType.CacesChariot,
                IsDelete = false
            });
            _people.Insert(new PersonModel
            {
                Nom = "Martin",
                Prenom = "Claire",
                Mail = "claire.martin@example.com",
                Service = ServiceType.Atelier,
                Qualification = QualificationType.CacesNacelle | QualificationType.CacesPontRoulant,
                IsDelete = false
            });
        }
    }

    public List<PersonModel> GetAllActive()
    {
        return _people.Find(p => p.IsDelete != true)
                      .OrderBy(p => p.Nom)
                      .ThenBy(p => p.Prenom)
                      .ToList();
    }

    public PersonModel Insert(PersonModel person)
    {
        // Let LiteDB assign Id if 0
        _people.Insert(person);
        return person;
    }

    public void Update(PersonModel person)
    {
        _people.Update(person);
    }

    public void SoftDelete(PersonModel person)
    {
        person.IsDelete = true;
        _people.Update(person);
    }

    public void Dispose()
    {
        _db?.Dispose();
    }
}
