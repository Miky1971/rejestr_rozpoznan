namespace Kurs.Rejestr;

public enum ClinicalStatus { Active, Cured, Relapse }
public enum ConfirmationStatus { Suspected, Confirmed }
public enum ExternalSystemKind { SysA, SysB }
public enum ReportStatus { Pending, Reported, Failed }

public class Diagnosis
{
    public Guid Id { get; set; }
    public ExternalSystemKind ExternalSystemKind { get; set; }
    // relacja do klasy Patient:
    public Guid PatientId { get; set; }
    public Patient Patient { get; set; }

    public required string ExternalSymbolDiagnosis { get; set; }
    public DateOnly DateDiagnosis { get; set; }
    public DateOnly? DateOnset { get; set; }
    public int? AgeOnset { get; set; }

    public required string Icd10Code { get; set; }
    public required string CodingSystem { get; set; }
    public required string Icd10Description { get; set; }
    public ClinicalStatus ClinicalStatus { get; set; }
    public ConfirmationStatus ConfirmationStatus { get; set; }

    public ReportStatus ReportStatus { get; set; }
}