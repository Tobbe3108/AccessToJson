using System.ComponentModel.DataAnnotations.Schema;

namespace AccessToJson;

public class TabMedarbejder
{
    public int Medarb { get; set; }
    public string Afdeling { get; set; }
    public string Navn { get; set; }
    public bool Skæring { get; set; }
    public bool VM { get; set; }
    public string Adresse { get; set; }
    public string Stilling { get; set; }
    public string Telefonnummer { get; set; }
    [Column("Aktivitet_x0020_1")] public string Aktivitet1 { get; set; }
    [Column("Aktivitet_x0020_2")] public string Aktivitet2 { get; set; }
    public string Skifttype { get; set; }
    public string Underafd { get; set; }
    public bool Aktiv { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    [Column("PW_x0020_expire")] public DateTime PWExpire { get; set; }
}