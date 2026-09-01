using System.Text.Json;
using System.Text.Json.Serialization;
using Kurs.Rejestr;
using Microsoft.EntityFrameworkCore;

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


file = "data.json";
List<RegisterDiagnosisRequest> dataDiagnoses = null!;
try
{
    string json = File.ReadAllText(file);
    dataDiagnoses = JsonSerializer.Deserialize<List<RegisterDiagnosisRequest>>(json, options);
    temp = $"Odczyt pliku: {file}, ilość rozpoznań: {dataDiagnoses.Count}, zapisane tymczasowo do 'dataDiagnoses'";
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

Console.WriteLine($"{++i}) Test danych wejściowych/rozpoznań z 'dataDiagnoses':");
foreach (var req in dataDiagnoses)
{
    var errors = Validator.Errors(req, icdValueSet, DateOnly.FromDateTime(DateTime.Now), context.Patients);
    if (errors.Count == 0) temp = "OK";
    else temp = string.Join(",\n            ", errors);
    Console.WriteLine("  " + req.ExternalSymbolDiagnosis + " " + temp);

    if (Validator.IsDuplicate(req, context.Diagnoses)) Console.WriteLine($"  {req.ExternalSymbolDiagnosis} już istnieje taka diagnoza z sytemu: {req.ExternalSystemKind}");
}

// od teraz aplikacja webowa (na 1. terminalu):
var builder = WebApplication.CreateBuilder(args);
builder.Services.ConfigureHttpJsonOptions(options => options.SerializerOptions.Converters.Add(new JsonStringEnumConverter())); // enumy jako tekst w JSON-ie wymagają specjalnej konfiguracji).
var app = builder.Build();

// test 0
// request: curl -X POST http://localhost:XXXX/diagnoses (na 2. terminalu)
// responce: app.MapPost("/diagnoses", () => Results.Ok($"{++i}) POST działa, kod {StatusCodes.Status200OK}"));

// test 1
// request: curl -i -X POST http://localhost:5000/diagnoses -H "Content-Type: application/json" -d @data-test-good.json
// responce: "7) Nowa  diagnoza z sytemu: SysA, o symbolu: REC-A-001 200"

// test 2
// request: curl -i -X POST http://localhost:5000/diagnoses -H "Content-Type: application/json" -d @data-test-bad.json 
// responce: {"type":"https://tools.ietf.org/html/rfc9110#section-15.5.1","title":"One or more validation errors occurred.","status":400,"errors":{"REC-TEST-002":["Data diagnozy nie może być z przyszłości: 01.01.2030","Kod Icd10: XYZ99, błędny lub nieaktywny"]}}
app.MapPost("/diagnoses", (RegisterDiagnosisRequest req) =>
{
    temp = $"diagnoza z sytemu: {req.ExternalSystemKind}, o symbolu: {req.ExternalSymbolDiagnosis}";
    Console.WriteLine($"{++i}) POST/diagnoses: {temp}");

    var errors = Validator.Errors(req, icdValueSet, DateOnly.FromDateTime(DateTime.Now), context.Patients);
    if (errors.Count > 0)
    {
        var error = new Dictionary<string, string[]> { [req.ExternalSymbolDiagnosis] = errors.ToArray() };
        Console.WriteLine($"{i}) POST/diagnoses: błędy w danych: {error.ToString}, (Status: {StatusCodes.Status400BadRequest})");
        return Results.ValidationProblem(error);
    }
    if (Validator.IsDuplicate(req, context.Diagnoses))
    {
        Console.WriteLine($"{i}) POST/diagnoses: istnieje już {temp}, (Status: {StatusCodes.Status200OK})");
        return Results.Ok($"{i}) Istnieje już {temp}, (Status: {StatusCodes.Status200OK})");
    }
    else
    {
        Diagnosis diagnosis = new Diagnosis()
        {
            Id = new Guid(),
            ExternalSystemKind = req.ExternalSystemKind,
            PatientId = Validator.FindPatient(req, context.Patients).Id,

            ExternalSymbolDiagnosis = req.ExternalSymbolDiagnosis,
            DateDiagnosis = req.DateDiagnosis,
            DateOnset = req.DateOnset,
            AgeOnset = req.AgeOnset,

            Icd10Code = req.Icd10Code,
            CodingSystem = req.CodingSystem,
            Icd10Description = icdValueSet.FindActiveDisease(req.Icd10Code).Description,
            ClinicalStatus = req.ClinicalStatus,
            ConfirmationStatus = req.ConfirmationStatus
        };
        context.Diagnoses.Add(diagnosis); // dodaj do kontekstu
        context.SaveChanges();            // EF Core wysyła odpowiednie SQL do bazy.
        Console.WriteLine($"{i}) Nowa {temp}, (Status: {StatusCodes.Status201Created})\n{i}) Diagnozy w bazie: {context.Diagnoses.Count()}");
        return Results.Created($"/diagnoses/{diagnosis.Id}", diagnosis);
    }
}
);


Console.WriteLine($"{++i}) Start serwera aplikacji:");
app.Run();


