using System.Globalization;

namespace Kurs.Rejestr;

public static class Validator
{
    public static List<string> Errors(RegisterDiagnosisRequest request, IcdValueSet codes, DateOnly date)
    {
        List<string> errors = new();

        if (request.DateDiagnosis > date) errors.Add("Data diagnozy nie może być z przyszlości");
        if (codes.FindActiveDisease(request.Icd10Code) == null) errors.Add("Kod Icd10 błędny lub nieaktywny");
        if (request.CodingSystem != codes.CodingSystem) errors.Add($"Błędny system kodowania, inny niż {codes.CodingSystem}");
        if ((request.DateOnset == null && request.AgeOnset == null) || (request.DateOnset != null && request.AgeOnset != null)) errors.Add("Brak jednocznacznego początku diagnozy.");

        return errors;
    }
}