using System.Collections;
using System.ComponentModel;
using System.Data.Common;
using System.IO.Pipelines;

namespace Kurs.Rejestr;

public static class DiagnosisRegistration
{
    private static readonly HttpClient httpClient = new HttpClient();
    private static string baseUrl = "";
    public static void Run(RegistersDbContext context, IcdValueSet icdValueSet, WebApplication app, string url)
    {
        string temp = "";
        baseUrl = url;

        // test 0
        // request: curl -X POST http://localhost:XXXX/diagnoses (na 2. terminalu)
        // responce: app.MapPost("/diagnoses", () => Results.Ok($"DiagnosisRegistration.Run: POST działa, kod {StatusCodes.Status200OK}"));

        // test 1
        // request: curl -i -X POST http://localhost:5000/diagnoses -H "Content-Type: application/json" -d @data-test-good.json
        // responce: "7) Nowa  diagnoza z sytemu: SysA, o symbolu: REC-A-001 200"

        // test 2
        // request: curl -i -X POST http://localhost:5000/diagnoses -H "Content-Type: application/json" -d @data-test-bad.json 
        // responce: {"type":"https://tools.ietf.org/html/rfc9110#section-15.5.1","title":"One or more validation errors occurred.","status":400,"errors":{"REC-TEST-002":["Data diagnozy nie może być z przyszłości: 01.01.2030","Kod Icd10: XYZ99, błędny lub nieaktywny"]}}
        app.MapPost("/diagnoses", async (RegisterDiagnosisRequest req) =>
        {
            temp = $"diagnoza z sytemu: {req.ExternalSystemKind}, o symbolu: {req.ExternalSymbolDiagnosis}";
            Console.WriteLine($"DiagnosisRegistration.Run: POST/diagnoses: {temp}");

            var errors = Validator.Errors(req, icdValueSet, DateOnly.FromDateTime(DateTime.Now), context.Patients);
            if (errors.Count > 0)
            {
                var error = new Dictionary<string, string[]> { [req.ExternalSymbolDiagnosis] = errors.ToArray() };
                Console.WriteLine($"DiagnosisRegistration.Run: POST/diagnoses: błędy w danych: {error.ToString}, (Status: {StatusCodes.Status400BadRequest})");
                return Results.ValidationProblem(error);
            }
            if (Validator.IsDuplicate(req, context.Diagnoses))
            {
                Console.WriteLine($"DiagnosisRegistration.Run: POST/diagnoses: istnieje już {temp}, (Status: {StatusCodes.Status200OK})");
                return Results.Ok($"DiagnosisRegistration.Run: Istnieje już {temp}, (Status: {StatusCodes.Status200OK})");
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
                Console.WriteLine($"DiagnosisRegistration.Run 1: Nowa {temp}, (Status diagnozy: {diagnosis.ReportStatus}) (Status operacji: {StatusCodes.Status201Created})\nDiagnosisRegistration.Run 1: Diagnozy w bazie: {context.Diagnoses.Count()}");
                diagnosis.ReportStatus = await DiagnosisRegistration.Report(diagnosis.Id, diagnosis.Icd10Code);
                context.SaveChanges();            // EF Core wysyła odpowiednie SQL do bazy.
                Console.WriteLine($"DiagnosisRegistration.Run 2: Nowa {temp}, (Status diagnozy: {diagnosis.ReportStatus})");
                return Results.Created($"/diagnoses/{diagnosis.Id}", diagnosis);
            }
        }
        );
    }
    public static async Task<ReportStatus> Report(Guid id, string icd10Code)
    {
        ExternalReport ourReport = new ExternalReport { Id = id, Icd10Code = icd10Code };
        for (int i = 0; i < 3; i++)
        {
            var responce = await httpClient.PostAsJsonAsync($"{baseUrl}/external-report", ourReport);
            Console.WriteLine($"DiagnosisRegistration.Report: Wysłanie raportu na zewnątrz po raz {i + 1}: {ourReport.Id}, {ourReport.Icd10Code}, ");
            if (responce.StatusCode == System.Net.HttpStatusCode.Accepted) return ReportStatus.Reported;
            else if (responce.StatusCode == System.Net.HttpStatusCode.BadRequest) return ReportStatus.Failed;
            if (i == 2) break;
            await Task.Delay(1000);
        }
        return ReportStatus.Failed;
    }
}

public record ExternalReport
{
    public Guid Id { get; init; }
    public string Icd10Code { get; init; }
}

public static class ExternalRegistry
{
    private static Dictionary<Guid, int> counter = new();
    public static void Run(WebApplication app)
    {
        string temp = "";
        app.MapPost("/external-report", (ExternalReport externalReport) =>
        {
            temp = $"Raport zewnętrzny: Klucz {externalReport.Id}, Wartość {externalReport.Icd10Code}";
            Console.WriteLine($"DiagnosisRegistration.Run: POST/external-report: {temp}");
            if (externalReport.Id == default || externalReport.Icd10Code == null || externalReport.Icd10Code == default) return Results.BadRequest();
            if (!counter.ContainsKey(externalReport.Id)) counter.Add(externalReport.Id, 1);
            else counter[externalReport.Id]++;
            if (counter[externalReport.Id] < 3) return Results.StatusCode(503);
            else return Results.Accepted();
        }
        );

    }
}
