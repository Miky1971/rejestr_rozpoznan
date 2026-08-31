namespace Kurs.Rejestr;

public class Patient
{
    public Guid Id { get; set; }
    public string? PESEL { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateOnly BirthDate { get; set; }
    public int Age { get; set; }
    public string Name => $"{this.FirstName} {this.LastName}";
    public ExternalSystemKind ExternalSystemKind { get; set; }
    public string? ExternalSymbolPatient { get; set; }

    // Add other properties as needed
    public Patient(DateOnly birthDate, string firstName, string lastName)
    {
        this.Id = Guid.NewGuid();
        this.BirthDate = birthDate;
        this.FirstName = firstName;
        this.LastName = lastName;

        var today = DateOnly.FromDateTime(DateTime.Now);
        this.Age = today.Year - this.BirthDate.Year;
        if (today < this.BirthDate.AddYears(this.Age)) this.Age--;
    }

    public override string ToString()
    {
        return $"Dane pacjenta: {Name}, data ur.: {BirthDate}, wiek: {Age}, Id: {Id}, ";
    }
}

public record struct BirthDate(DateOnly Date)
{
    public int Age => CalculateAge(Date);

    private static int CalculateAge(DateOnly birthDate)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var age = today.Year - birthDate.Year;

        if (birthDate > today.AddYears(-age))
        {
            age--;
        }

        return age;
    }
}
