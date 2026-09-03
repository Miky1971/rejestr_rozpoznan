using System.Text.RegularExpressions;
using System;
using Dapper;
using Microsoft.Data.Sqlite;

namespace Kurs.Rejestr;

public static class PatientReports
{
    private static readonly HttpClient httpClient = new HttpClient();
    private static string baseUrl = "";
    public static void PatientSearch(RegistersDbContext context, WebApplication app)
    {
        app.MapGet("/patient", (string? pesel, string? symbol, ExternalSystemKind? system) =>
        {
            Patient patient = context.Patients.FirstOrDefault(p => (pesel != null && p.PESEL == pesel) || (system != null && p.ExternalSystemKind == system && symbol != null && p.ExternalSymbolPatient == symbol));
            if (patient == null) return Results.NotFound();
            else return Results.Ok(patient);
        });
    }

    public static void PatientDiagnoses(RegistersDbContext context, WebApplication app)
    {
        app.MapGet("/patient/{patientId}/diagnoses", (Guid patientId, ClinicalStatus? status, int page = 1, int pageSize = 20) =>
        {
            List<Diagnosis> diagnoses = context.Diagnoses.Where(d => d.PatientId == patientId)
                .Where(d => (status == null || d.ClinicalStatus == status))
                .OrderBy(d => d.DateOnset)
                .Skip((page - 1) * pageSize)
                .Take(Math.Min(50, pageSize))
                .ToList();
            if (diagnoses.Count == 0) return Results.NotFound();
            else return Results.Ok(diagnoses);
        });
    }

    public static void SummaryIcd10Code(SqliteConnection connectionDB, WebApplication app)
    {
        app.MapGet("/summary", () =>
        {
            var summary = connectionDB.Query<Icd10Summary>("SELECT Icd10Code, COUNT(*) count FROM Diagnoses GROUP BY Icd10Code ORDER BY COUNT(*) DESC");
            if (summary.Count() == 0) return Results.NotFound();
            else return Results.Ok(summary);
        });
    }
}

public record Icd10Summary(string Icd10Code, long Count);
