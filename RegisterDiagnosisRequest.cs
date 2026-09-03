namespace Kurs.Rejestr;

public record RegisterDiagnosisRequest
{
    public ExternalSystemKind ExternalSystemKind { get; init; }
    public string? ExternalSymbolPatient { get; init; }
    public string? PESEL { get; init; }

    public required string ExternalSymbolDiagnosis { get; init; }
    public DateOnly DateDiagnosis { get; init; }
    public DateOnly? DateOnset { get; init; }
    public int? AgeOnset { get; init; }

    public required string Icd10Code { get; init; }
    public required string CodingSystem { get; init; }
    public required string Icd10Description { get; init; }
    public ClinicalStatus ClinicalStatus { get; init; }
    public ConfirmationStatus ConfirmationStatus { get; init; }
};