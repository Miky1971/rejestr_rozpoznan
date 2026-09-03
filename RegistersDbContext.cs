namespace Kurs.Rejestr;

using Microsoft.EntityFrameworkCore;

public class RegistersDbContext : DbContext
{
    private readonly string dbConnectionString;

    public RegistersDbContext(string dbConnectionString)
    {
        this.dbConnectionString = dbConnectionString;
    }

    public DbSet<Patient> Patients { get; set; }
    public DbSet<Diagnosis> Diagnoses { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite(dbConnectionString);
    }
}
