namespace Kurs.Rejestr;

public class IcdValueSet
{
    public required string CanonicalUri { get; set; }
    public required string CodingSystem { get; set; }
    public required List<IcdCode> Codes { get; set; }
    public IcdCode? FindActiveDisease(string code)
    {
        return this.Codes.FirstOrDefault(c => c.Code == code && !c.Retired);
    }
}

public class IcdCode
{
    public required string Code { get; set; }
    public required string Description { get; set; }
    public bool Retired { get; set; }

}
