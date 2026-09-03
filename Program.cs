using System.Text.Json;
using System.Text.Json.Serialization;
using Kurs.Rejestr;
using Microsoft.EntityFrameworkCore;

Console.WriteLine($"START");
RegistersDbContext context = DataBase.Run();

JsonSerializerOptions options = new JsonSerializerOptions
{
    PropertyNameCaseInsensitive = true
};
options.Converters.Add(new JsonStringEnumConverter());
IcdValueSet icdValueSet = SicknessCodes.Download("data/icd10.json", options);

DataTest.Run(context, icdValueSet, options);

// odtąd budowanie aplikacji webowej 
var builder = WebApplication.CreateBuilder(args);
builder.Services.ConfigureHttpJsonOptions(options => options.SerializerOptions.Converters.Add(new JsonStringEnumConverter())); // enumy jako tekst w JSON-ie wymagają specjalnej konfiguracji).
var app = builder.Build();
string baseUrl = app.Urls.FirstOrDefault() ?? "http://localhost:5000"; // przechwytywanie nr portu, żeby w zapytaniach POST nie był na sztywno

DiagnosisRegistration.Run(context, icdValueSet, app, baseUrl);
ExternalRegistry.Run(app);
PatientReports.PatientSearch(context, app);
PatientReports.PatientDiagnoses(context, app);








Console.WriteLine($"Start serwera aplikacji:");
app.Run(); // uruchamiana na 1. terminalu

static class DataBase
{
    public static RegistersDbContext Run()
    {
        const string temp = "DataBase.Run:";
        var context = new RegistersDbContext();
        context.Database.Migrate();
        Console.WriteLine($"{temp} Migracja BD Registers");

        if (!context.Patients.Any())
        {
            Patient p1 = new Patient(new DateOnly(1970, 5, 12), "Jan", "Kowalski") { ExternalSystemKind = ExternalSystemKind.SysA, ExternalSymbolPatient = "K-100", PESEL = null };
            Patient p2 = new Patient(new DateOnly(1982, 11, 3), "Anna", "Nowak") { ExternalSystemKind = ExternalSystemKind.SysB, ExternalSymbolPatient = "K-100", PESEL = null };
            Patient p3 = new Patient(new DateOnly(1995, 7, 20), "Piotr", "Wiśniewski") { ExternalSystemKind = ExternalSystemKind.SysB, ExternalSymbolPatient = "K-200", PESEL = null };
            Patient p4 = new Patient(new DateOnly(1985, 1, 1), "Katarzyna", "Zielińska") { ExternalSystemKind = ExternalSystemKind.SysA, ExternalSymbolPatient = null, PESEL = "85010112345" };
            context.Patients.AddRange(p1, p2, p3, p4);
            context.SaveChanges();
            Console.WriteLine($"{temp} Wstawianie uzytkowników do bazy");
        }
        else Console.WriteLine($"{temp} Pacjęci w bazie: {context.Patients.Count()}");
        Console.WriteLine($"{temp} Diagnozy w bazie: {context.Diagnoses.Count()}");
        return context;
    }
}

static class SicknessCodes
{
    public static IcdValueSet Download(string file, JsonSerializerOptions options)
    {
        bool ok = false;
        string temp = "";
        IcdValueSet icdValueSet = null!;
        try
        {
            string json = File.ReadAllText(file);
            icdValueSet = JsonSerializer.Deserialize<IcdValueSet>(json, options);
            temp = $"Odczyt pliku: {file}, ilość kodów chorobowych: {icdValueSet.Codes.Count}, zapisane tymczasowo do 'icdValueSet'";
            ok = true;
        }
        catch (FileNotFoundException ex)
        {
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
            Console.WriteLine($"SicknessCodes.Download: {temp}");
            if (!ok) Environment.Exit(1);
        }
        return icdValueSet;
    }
}