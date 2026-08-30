namespace Kurs.Models;

public class Patient
{
    public Guid Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateOnly BirthDate { get; set; }
    public int Age { get; set; }
    public string Name => $"{this.FirstName} {this.LastName}";

    // Add other properties as needed
    public Patient(DateTime birthDate, string firstName, string lastName)
    {
        this.Id = Guid.NewGuid();
        this.BirthDate = birthDate;
        this.Age = (int)((DateTime.Now - this.BirthDate).Days / 365.25);
        this.FirstName = firstName;
        this.LastName = lastName;
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
