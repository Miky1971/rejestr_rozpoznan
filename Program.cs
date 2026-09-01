using System.Text.Json;
using System.Text.Json.Serialization;
using Kurs.Rejestr;
using Microsoft.EntityFrameworkCore;

string temp = "";
int i = 0;
bool ok = true;
Console.WriteLine($"START");

var context = new RegistersDbContext();
context.Database.Migrate();
Console.WriteLine($"{++i}) Migracja BD Registers: {ok}");

if (!context.Patients.Any())
{
    Patient p1 = new Patient(new DateOnly(1970, 5, 12), "Jan", "Kowalski") { ExternalSystemKind = ExternalSystemKind.SysA, ExternalSymbolPatient = "K-100", PESEL = null };
    Patient p2 = new Patient(new DateOnly(1982, 11, 3), "Anna", "Nowak") { ExternalSystemKind = ExternalSystemKind.SysB, ExternalSymbolPatient = "K-100", PESEL = null };
    Patient p3 = new Patient(new DateOnly(1995, 7, 20), "Piotr", "Wiśniewski") { ExternalSystemKind = ExternalSystemKind.SysB, ExternalSymbolPatient = "K-200", PESEL = null };
    Patient p4 = new Patient(new DateOnly(1985, 1, 1), "Katarzyna", "Zielińska") { ExternalSystemKind = ExternalSystemKind.SysA, ExternalSymbolPatient = null, PESEL = "85010112345" };
    context.Patients.AddRange(p1, p2, p3, p4);
    context.SaveChanges();
    Console.WriteLine($"{++i}) Wstawianie uzytkowników do bazy");
}
else Console.WriteLine($"{++i}) Pacjęci w bazie: {context.Patients.Count()}");
Console.WriteLine($"{++i}) Diagnozy w bazie: {context.Diagnoses.Count()}");

string file = "icd10.json";
IcdValueSet icdValueSet = null!;
JsonSerializerOptions options = new JsonSerializerOptions
{
    PropertyNameCaseInsensitive = true
};
options.Converters.Add(new JsonStringEnumConverter());
try
{
    string json = File.ReadAllText(file);
    icdValueSet = JsonSerializer.Deserialize<IcdValueSet>(json, options);
    temp = $"Odczyt pliku: {file}, ilość kodów chorobowych: {icdValueSet.Codes.Count}, zapisane tymczasowo do 'icdValueSet'";
    ok = true;
}
catch (FileNotFoundException ex)
{
    temp = $"Brak pliku: {file}.\n{ex.Message} ({ex.StackTrace})";
    temp = $"Brak pliku: {file}.\n{ex.Message}";
    ok = false;
}
catch (JsonException ex)
{
    temp = $"Odczyt pliku: {file}: błędny format JSON\n{ex.Message}";
    ok = false;
}
catch (Exception ex)
{
    temp = $"Odczyt pliku: {file} nie udał się.\n{ex.Message}";
    ok = false;
}
finally
{
    Console.WriteLine($"{++i}) {temp}");
    if (!ok) Environment.Exit(1);
}

DataTest.Run(context, icdValueSet, options);

// od teraz aplikacja webowa (na 1. terminalu):
var builder = WebApplication.CreateBuilder(args);
builder.Services.ConfigureHttpJsonOptions(options => options.SerializerOptions.Converters.Add(new JsonStringEnumConverter())); // enumy jako tekst w JSON-ie wymagają specjalnej konfiguracji).
var app = builder.Build();

MapPostDiagnosisEndpoints.Run(context, icdValueSet, app);

Console.WriteLine($"{++i}) Start serwera aplikacji:");
app.Run();
