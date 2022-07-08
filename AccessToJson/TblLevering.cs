namespace AccessToJson;

public class TblLevering
{
    public int Levnr { get; set; }
    public int Linie { get; set; }
    public string Pallenr { get; set; }
    public DateTime LevDato { get; set; }
    public DateTime AfsDato { get; set; }
    public string BatchNr { get; set; }
    public DateTime RubDato { get; set; }
    public float Maengde { get; set; }
    public DateTime GodkLabD { get; set; }
    public string Bem { get; set; }
    public int Farve { get; set; }
}