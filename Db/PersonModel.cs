using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace BlazorServerApp.Db;

public class PersonModel
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Nom { get; set; }

    [Required, MaxLength(100)]
    public string Prenom { get; set; }

    [Required]
    public ServiceType Service { get; set; }

    [Required]
    public QualificationType Qualification { get; set; }
}

public enum ServiceType
{
    [Display(Name = "Bureau d'étude")] BureauDEtude,
    [Display(Name = "Atelier")] Atelier,
    [Display(Name = "Direction")] Direction,
    [Display(Name = "Automatisme")] Automatisme,
    [Display(Name = "Maintenance")] Maintenance
}

[Flags]
public enum QualificationType
{
    None = 0,
    [Display(Name = "CACES chariot")] CacesChariot = 1 << 0,
    [Display(Name = "CACES grue")] CacesGrue = 1 << 1,
    [Display(Name = "CACES nacelle")] CacesNacelle = 1 << 2,
    [Display(Name = "CACES pont roulant")] CacesPontRoulant = 1 << 3
}

public static class EnumDisplayExtensions
{
    public static string GetDisplayName(this Enum value)
    {
        var member = value.GetType().GetMember(value.ToString());
        if (member.Length > 0)
        {
            var attr = member[0].GetCustomAttribute<DisplayAttribute>();
            if (attr != null && !string.IsNullOrWhiteSpace(attr.Name))
            {
                return attr.Name!;
            }
        }
        return value.ToString();
    }

    public static IEnumerable<string> GetFlagsDisplayNames<TEnum>(this TEnum value) where TEnum : struct, Enum
    {
        foreach (var flag in Enum.GetValues<TEnum>())
        {
            if (Convert.ToInt64(flag) == 0) continue; // skip None
            var asEnum = (Enum)(object)flag;
            if (((Enum)(object)value).HasFlag(asEnum))
            {
                yield return asEnum.GetDisplayName();
            }
        }
    }
}