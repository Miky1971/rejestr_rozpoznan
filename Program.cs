using System.Text.Json;
using Kurs.Rejestr;
using Microsoft.EntityFrameworkCore;

int i = 0;
bool ok = true;
Console.WriteLine($"{i}) Build: {ok}");

var context = new RegistersDbContext();
context.Database.Migrate();
Console.WriteLine($"{++i}) Migracja BD Registers: {ok}");

string temp = "";
string file = "icd10.json";
IcdValueSet icdValueSet = null!;
JsonSerializerOptions options = new JsonSerializerOptions
{
    PropertyNameCaseInsensitive = true
};
try
{
    string json = File.ReadAllText(file);
    icdValueSet = JsonSerializer.Deserialize<IcdValueSet>(json, options);
    temp = $"Odczyt pliku: {file}, Ilość kodów: {icdValueSet.Codes.Count}";
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

