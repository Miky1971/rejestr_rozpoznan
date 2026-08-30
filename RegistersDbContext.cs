namespace Kurs.Rejestr;

using Microsoft.EntityFrameworkCore;

public class RegistersDbContext : DbContext
{
    public DbSet<Patient> Patients { get; set; }
    public DbSet<Diagnosis> Diagnoses { get; set; }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=registers.db");
    }
}