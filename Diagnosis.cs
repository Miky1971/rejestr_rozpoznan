public enum ClinicalStatus { active, cured, relapse }
public enum ConfirmationStatus { suspected, confirmed }
public enum SystemKind { sys_a, sys_b }
public class Diagnosis
{
    public Guid Id { get; set; }
    public Guid Patient_Id { get; set; }
    public string CodingSystem { get; set; }
    public string Icd10Code { get; set; }
    public string Icd10Description { get; set; }
    public DateOnly DateDiagnosis { get; set; }
    public int AgeDiagnosis { get; set; }
    public ClinicalStatus ClinicalStatus { get; set; }
    public ConfirmationStatus ConfirmationStatus { get; set; }
    public SystemKind SystemKind { get; set; }


}