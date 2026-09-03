namespace Kurs.Rejestr;

using System.Text.Json;
using System.Text.Json.Serialization;

public static class DataTest
{

    public static void Run(RegistersDbContext context, IcdValueSet icdValueSet, JsonSerializerOptions options)
    {
        bool ok = false;
        string temp = "";
        string file = "data/data.json";
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
            Console.WriteLine($"DataTest.Run: {temp}");
            if (!ok) Environment.Exit(1);
        }

        Console.WriteLine($"DataTest.Run: Test danych wejściowych/rozpoznań z 'dataDiagnoses':");
        foreach (var req in dataDiagnoses)
        {
            var errors = Validator.Errors(req, icdValueSet, DateOnly.FromDateTime(DateTime.Now), context.Patients);
            if (errors.Count == 0) temp = "OK";
            else temp = string.Join(",\n            ", errors);
            Console.WriteLine("  " + req.ExternalSymbolDiagnosis + " " + temp);

            if (Validator.IsDuplicate(req, context.Diagnoses)) Console.WriteLine($"  {req.ExternalSymbolDiagnosis} już istnieje taka diagnoza z sytemu: {req.ExternalSystemKind}");
        }

    }
}