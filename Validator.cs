using System.Globalization;
using Microsoft.EntityFrameworkCore;

namespace Kurs.Rejestr;

public static class Validator
{
    public static List<string> Errors(RegisterDiagnosisRequest request, IcdValueSet codes, DateOnly date, DbSet<Patient> patients)
    {
        List<string> errors = new();

        if (FindPatient(request, patients) == null) errors.Add($"Brak pacjenta z nr: {request.ExternalSymbolPatient}, z sytemu {request.ExternalSystemKind} lub z PESEL: {request.PESEL}");
        if (request.DateDiagnosis > date) errors.Add($"Data diagnozy nie może być z przyszłości: {request.DateDiagnosis}");
        if (codes.FindActiveDisease(request.Icd10Code) == null) errors.Add($"Kod Icd10: {request.Icd10Code}, błędny lub nieaktywny");
        if (request.CodingSystem != codes.CodingSystem) errors.Add($"Błędny system kodowania: {request.CodingSystem}, inny niż {codes.CodingSystem}");
        if ((request.DateOnset == null && request.AgeOnset == null) || (request.DateOnset != null && request.AgeOnset != null)) errors.Add($"Brak jednocznacznego początku diagnozy: {request.DateOnset} i {request.AgeOnset}");

        return errors;
    }

    public static bool IsDuplicate(RegisterDiagnosisRequest request, DbSet<Diagnosis> diagnosis)
    {
        return diagnosis.Any(d => (request.ExternalSystemKind == d.ExternalSystemKind && request.ExternalSymbolDiagnosis == d.ExternalSymbolDiagnosis) ? true : false);
    }

    public static Patient? FindPatient(RegisterDiagnosisRequest request, DbSet<Patient> patients)
    {
        return patients.FirstOrDefault(p => ((p.PESEL != null && request.PESEL != null && p.PESEL == request.PESEL) ||
            (p.ExternalSymbolPatient != null && request.ExternalSymbolPatient != null && p.ExternalSystemKind == request.ExternalSystemKind && p.ExternalSymbolPatient == request.ExternalSymbolPatient)));
    }
}