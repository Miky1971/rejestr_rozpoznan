using System.Text.Json;
using System.Text.Json.Serialization;
using Kurs.Rejestr;
using Microsoft.EntityFrameworkCore;

int i = 0;
bool ok = true;
Console.WriteLine($"{i}) Build: {ok}");

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

string temp = "";
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
    temp = $"Odczyt pliku: {file}, ilość kodów: {icdValueSet.Codes.Count}";
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


file = "data.json";
List<RegisterDiagnosisRequest> dataDiagnoses = null!;
try
{
    string json = File.ReadAllText(file);
    dataDiagnoses = JsonSerializer.Deserialize<List<RegisterDiagnosisRequest>>(json, options);
    temp = $"Odczyt pliku: {file}, ilość kodów: {dataDiagnoses.Count}";
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

Console.WriteLine($"{++i}) Test danych wejściowych");
foreach (var req in dataDiagnoses)
{
    var errors = Validator.Errors(req, icdValueSet, DateOnly.FromDateTime(DateTime.Now));
    if (errors.Count == 0) temp = "OK";
    else temp = string.Join(", ", errors);
    Console.WriteLine(req.ExternalSymbolDiagnosis + " " + temp);
}

/*
// od teraz aplikacja webowa:
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// request: curl -X POST http://localhost:XXXX/diagnoses
// responce:
app.MapPost("/diagnoses", () => Results.Ok($"{++i}) POST działa, kod {StatusCodes.Status200OK}"));

Console.WriteLine($"{++i}) Start serwera aplikacji:");
app.Run();
*/
