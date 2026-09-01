namespace Kurs.Rejestr;

public static class MapPostDiagnosisEndpoints
{
    public static void Run(RegistersDbContext context, IcdValueSet icdValueSet, WebApplication app)
    {
        int i = 0;
        string temp = "";

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
            Console.WriteLine($"Pd{++i}) POST/diagnoses: {temp}");

            var errors = Validator.Errors(req, icdValueSet, DateOnly.FromDateTime(DateTime.Now), context.Patients);
            if (errors.Count > 0)
            {
                var error = new Dictionary<string, string[]> { [req.ExternalSymbolDiagnosis] = errors.ToArray() };
                Console.WriteLine($"Pd{i}) POST/diagnoses: błędy w danych: {error.ToString}, (Status: {StatusCodes.Status400BadRequest})");
                return Results.ValidationProblem(error);
            }
            if (Validator.IsDuplicate(req, context.Diagnoses))
            {
                Console.WriteLine($"Pd{i}) POST/diagnoses: istnieje już {temp}, (Status: {StatusCodes.Status200OK})");
                return Results.Ok($"Pd{i}) Istnieje już {temp}, (Status: {StatusCodes.Status200OK})");
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
                Console.WriteLine($"Pd{i}) Nowa {temp}, (Status: {StatusCodes.Status201Created})\nPd{i}) Diagnozy w bazie: {context.Diagnoses.Count()}");
                return Results.Created($"/diagnoses/{diagnosis.Id}", diagnosis);
            }
        }
        );

    }
}
