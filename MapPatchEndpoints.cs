using System.Security.Cryptography.X509Certificates;

namespace Kurs.Rejestr;

public static class DiagnosisUpdate
{
    public static void StatusChange(RegistersDbContext context, WebApplication app)
    {
        app.MapPatch("/diagnoses/{diagnosisId}", (Guid diagnosisId, ClinicalStatus newStatus) =>
                {
                    Diagnosis diagnosis = context.Diagnoses.Find(diagnosisId);
                    if (diagnosis == null) return Results.NotFound();
                    else if (diagnosis.ClinicalStatus == ClinicalStatus.Cured && newStatus == ClinicalStatus.Active) return Results.Conflict();
                    else
                    {
                        diagnosis.ClinicalStatus = newStatus;
                        context.SaveChanges(); // EF Core wysyła odpowiednie SQL do bazy.
                        return Results.Ok(diagnosis);
                    }
                });
    }
}
