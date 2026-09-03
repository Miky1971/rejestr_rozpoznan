## Założenia projektowe
Pacjenci są danymi startowymi, a nie tworzeni przez endpoint (zadanie tego nie wymaga).

Trzy reguły walidacji przy rejestracji rozpoznania (sekcja "Rejestracja rozpoznania") dotyczą wyłącznie:
- daty postawienia, 
- kodu ICD-10 
- opisu słownego — zadanie nie wymaga tworzenia nowego pacjenta jako część tej operacji.

## Validator — osobna klasa do walidacji:
# public static List<string> Errors(RegisterDiagnosisRequest request, IcdValueSet codes, DateOnly date, DbSet<Patient> patients)
metoda sprawdzająca cztery reguły naraz (data, kod+system, dokładnie jedno z dwóch pól początku), zwracająca listę błędów

# public static Patient? FindPatient(RegisterDiagnosisRequest request, DbSet<Patient> patients)
wyszukanie pacjenta — po PESEL albo (ExternalSymbolPatient+ExternalSystemKind) w context.Patients.

# public static bool IsDuplicate(RegisterDiagnosisRequest request, DbSet<Diagnosis> diagnosis)
sprawdzenie czy taka para: ExternalSymbolDiagnosis + ExternalSystemKind już istnieje w BD

## Błędy które testuje w `data.json`
1. poprawna rejestracja (to nie błąd): 5 (poprawnych, różne kombinacje pacjentów/początku dolegliwości) 
2. duplikat - to samo rozpoznanie wysłane drugi raz (ma się udać, nie zwrócić błędu — test idempotencji)
3. data postawienia rozpoznania z przyszłości
4. kod ICD-10, którego nie ma w zbiorze
5. kod wycofany (`Z00`)
6. kod podany z obcym systemem kodowania
7. początek dolegliwości: podane oba naraz (data i wiek) — zabronione wprost w treści zadania
8. początek dolegliwości: nie podane żadne z dwóch
9. pacjent nie istnieje: błędny PESEL
10. pacjent nie istnieje: błędny numer zewnętrznej kartoteki
kilka naruszeń naraz: 
11. jeden przypadek z 2 błędami (data + kod)
12. i z 4 naraz (data + kod + system + oba pola początku)


# Błedy które jeszcze można by kiedyś testować (na razie odłożone)
- brak externalSystemKind albo inny, nieobsługiwany

## testowanie
Dwa pliki gotowe do testów z osobnych plików:
    curl -i -X POST http://localhost:5000/diagnoses -H "Content-Type: application/json" -d @data/data-test-good.json
    curl -i -X POST http://localhost:5000/diagnoses -H "Content-Type: application/json" -d @data/data-test-bad.json

## 12 scenariuszy dla POST /diagnoses (te same reguły co w data/data.json, tu jako osobne żądania curl)

1. Poprawna rejestracja → 201:
curl -i -X POST http://localhost:5000/diagnoses -H "Content-Type: application/json" -d '{"externalSystemKind":"SysA","externalSymbolPatient":"K-100","pesel":null,"externalSymbolDiagnosis":"REC-DOC-01","dateDiagnosis":"2026-08-15","dateOnset":null,"ageOnset":30,"icd10Code":"J45","codingSystem":"ICD-10","icd10Description":"x","clinicalStatus":"Active","confirmationStatus":"Confirmed"}'

2. Ten sam rekord drugi raz (idempotencja) → 200:
curl -i -X POST http://localhost:5000/diagnoses -H "Content-Type: application/json" -d '{"externalSystemKind":"SysA","externalSymbolPatient":"K-100","pesel":null,"externalSymbolDiagnosis":"REC-DOC-01","dateDiagnosis":"2026-08-15","dateOnset":null,"ageOnset":30,"icd10Code":"J45","codingSystem":"ICD-10","icd10Description":"x","clinicalStatus":"Active","confirmationStatus":"Confirmed"}'

3. Data z przyszłości → 400:
curl -i -X POST http://localhost:5000/diagnoses -H "Content-Type: application/json" -d '{"externalSystemKind":"SysA","externalSymbolPatient":"K-100","pesel":null,"externalSymbolDiagnosis":"REC-DOC-03","dateDiagnosis":"2030-01-01","dateOnset":null,"ageOnset":30,"icd10Code":"J45","codingSystem":"ICD-10","icd10Description":"x","clinicalStatus":"Active","confirmationStatus":"Confirmed"}'

4. Kod nieobecny w zbiorze → 400:
curl -i -X POST http://localhost:5000/diagnoses -H "Content-Type: application/json" -d '{"externalSystemKind":"SysA","externalSymbolPatient":"K-100","pesel":null,"externalSymbolDiagnosis":"REC-DOC-04","dateDiagnosis":"2026-08-15","dateOnset":null,"ageOnset":30,"icd10Code":"XYZ99","codingSystem":"ICD-10","icd10Description":"x","clinicalStatus":"Active","confirmationStatus":"Confirmed"}'

5. Kod wycofany (Z00) → 400:
curl -i -X POST http://localhost:5000/diagnoses -H "Content-Type: application/json" -d '{"externalSystemKind":"SysA","externalSymbolPatient":"K-100","pesel":null,"externalSymbolDiagnosis":"REC-DOC-05","dateDiagnosis":"2026-08-15","dateOnset":null,"ageOnset":30,"icd10Code":"Z00","codingSystem":"ICD-10","icd10Description":"x","clinicalStatus":"Active","confirmationStatus":"Confirmed"}'

6. Zły system kodowania → 400:
curl -i -X POST http://localhost:5000/diagnoses -H "Content-Type: application/json" -d '{"externalSystemKind":"SysA","externalSymbolPatient":"K-100","pesel":null,"externalSymbolDiagnosis":"REC-DOC-06","dateDiagnosis":"2026-08-15","dateOnset":null,"ageOnset":30,"icd10Code":"J45","codingSystem":"SNOMED","icd10Description":"x","clinicalStatus":"Active","confirmationStatus":"Confirmed"}'

7. Oba pola początku dolegliwości naraz → 400:
curl -i -X POST http://localhost:5000/diagnoses -H "Content-Type: application/json" -d '{"externalSystemKind":"SysA","externalSymbolPatient":"K-100","pesel":null,"externalSymbolDiagnosis":"REC-DOC-07","dateDiagnosis":"2026-08-15","dateOnset":"2020-01-01","ageOnset":30,"icd10Code":"J45","codingSystem":"ICD-10","icd10Description":"x","clinicalStatus":"Active","confirmationStatus":"Confirmed"}'

8. Żadne pole początku dolegliwości → 400:
curl -i -X POST http://localhost:5000/diagnoses -H "Content-Type: application/json" -d '{"externalSystemKind":"SysA","externalSymbolPatient":"K-100","pesel":null,"externalSymbolDiagnosis":"REC-DOC-08","dateDiagnosis":"2026-08-15","dateOnset":null,"ageOnset":null,"icd10Code":"J45","codingSystem":"ICD-10","icd10Description":"x","clinicalStatus":"Active","confirmationStatus":"Confirmed"}'

9. Zły PESEL (brak pacjenta) → 400:
curl -i -X POST http://localhost:5000/diagnoses -H "Content-Type: application/json" -d '{"externalSystemKind":"SysA","externalSymbolPatient":null,"pesel":"99999999999","externalSymbolDiagnosis":"REC-DOC-09","dateDiagnosis":"2026-08-15","dateOnset":null,"ageOnset":30,"icd10Code":"J45","codingSystem":"ICD-10","icd10Description":"x","clinicalStatus":"Active","confirmationStatus":"Confirmed"}'

10. Zły numer zewnętrzny (brak pacjenta) → 400:
curl -i -X POST http://localhost:5000/diagnoses -H "Content-Type: application/json" -d '{"externalSystemKind":"SysA","externalSymbolPatient":"K-999","pesel":null,"externalSymbolDiagnosis":"REC-DOC-10","dateDiagnosis":"2026-08-15","dateOnset":null,"ageOnset":30,"icd10Code":"J45","codingSystem":"ICD-10","icd10Description":"x","clinicalStatus":"Active","confirmationStatus":"Confirmed"}'

11. 2 błędy naraz (data + kod) → 400, oba błędy w jednej odpowiedzi:
curl -i -X POST http://localhost:5000/diagnoses -H "Content-Type: application/json" -d '{"externalSystemKind":"SysA","externalSymbolPatient":"K-100","pesel":null,"externalSymbolDiagnosis":"REC-DOC-11","dateDiagnosis":"2030-01-01","dateOnset":null,"ageOnset":30,"icd10Code":"XYZ99","codingSystem":"ICD-10","icd10Description":"x","clinicalStatus":"Active","confirmationStatus":"Confirmed"}'

12. Maksimum błędów naraz (4: data + kod + system + oba pola początku) → 400, wszystkie 4 błędy w jednej odpowiedzi:
curl -i -X POST http://localhost:5000/diagnoses -H "Content-Type: application/json" -d '{"externalSystemKind":"SysA","externalSymbolPatient":"K-100","pesel":null,"externalSymbolDiagnosis":"REC-DOC-12","dateDiagnosis":"2030-01-01","dateOnset":"2020-01-01","ageOnset":30,"icd10Code":"XYZ99","codingSystem":"SNOMED","icd10Description":"x","clinicalStatus":"Active","confirmationStatus":"Confirmed"}'

## Zgłoszenie rozpoznania do rejestru sprawozdawczego...
// zapytania POST "/diagnoses", async (RegisterDiagnosisRequest req)
DiagnosisRegistration.Run(context, icdValueSet, app, baseUrl);

# ... i wysłanie raportu o nowo zarejestrowanym rozpoznaniu do atrapy zewnętrznego endpoitu 
# zawsze 3 próby: Pierwsze dwie próby odrzuca kodem 503, kolejne 202. 
# Zgłoszenie bez identyfikatora rozpoznania albo bez kodu odrzuca kodem 400.
// zapytania POST "/external-report", (ExternalReport externalReport)
ExternalRegistry.Run(app);

## Odczyt danych z bazy SQLite: 
# wyszukiwanie pacjenta po numerze PESEL albo po numerze kartoteki: 
// zapytania GET /patient (string? pesel, string? symbol, ExternalSystemKind? system)
PatientReports.PatientSearch(context, app);

Po PESEL → 200, Katarzyna Zielińska:
curl -s -i "http://localhost:5000/patient?pesel=85010112345"

Po symbolu+SysA (K-100) → 200, Jan Kowalski:
curl -s -i "http://localhost:5000/patient?symbol=K-100&system=SysA"

Po symbolu+SysB (K-100) → 200, Anna Nowak:
curl -s -i "http://localhost:5000/patient?symbol=K-100&system=SysB"

Nieistniejący PESEL → 404:
curl -s -i "http://localhost:5000/patient?pesel=00000000000"

# odczyt rozpoznań wskazanego pacjenta (z filtrem po statusie klinicznym, sortowaniem po dacie postawienia i stronicowana): 
// zapytania GET /patient/{patientId}/diagnoses (Guid patientId, ClinicalStatus? status, int page = 1, int pageSize = 20)
PatientReports.PatientDiagnoses(context, app);

Domyślne bez filtra i stronicowania, dla Piotra → 200 (obie diagnozy Piotra):
curl -s -i "http://localhost:5000/patient/76299a30-b0a8-474d-a771-dd2bcb5e8ea8/diagnoses"

Z filtrem status=Active → 200, (obie Active):
curl -s -i "http://localhost:5000/patient/76299a30-b0a8-474d-a771-dd2bcb5e8ea8/diagnoses?status=Active"

Stronicowanie page=1&pageSize=1 → 200, (tylko jedna z dwóch):
curl -s -i "http://localhost:5000/patient/76299a30-b0a8-474d-a771-dd2bcb5e8ea8/diagnoses?page=1&pageSize=1"

Nieistniejący patientId → 404 (brak danych):
curl -s -i "http://localhost:5000/patient/00000000-0000-0000-0000-000000000000/diagnoses"

Status, którego Piotr NIE ma (status=Cured) → 404 (brak danych):
curl -s -i "http://localhost:5000/patient/76299a30-b0a8-474d-a771-dd2bcb5e8ea8/diagnoses?status=Cured"
brak danych = 404 (w obu przypadkach, z powodu braku pacjenta lub braku wyników z takim statusem)

## Policz liczbę rozpoznań w podziale na kod ICD-10, malejąco (zapytanie w SQL i drugie połaczenie do BD - dla Dapper'a - using var connectionDB = new SqliteConnection(dbConnectionString);) :
// zapytanie GET /summary
PatientReports.SummaryIcd10Code(connectionDB, app);

curl -s -i "http://localhost:5000/summary"
[{"icd10Code":"M54","count":1},{"icd10Code":"K21","count":1},{"icd10Code":"J45","count":1},{"icd10Code":"I10","count":1},{"icd10Code":"E11","count":1}]


## Zmiana statusu klinicznego rozpoznania: 
// zapytania PATCH /diagnoses/{diagnosisId}?newStatus=...
DiagnosisUpdate.StatusChange(context, app);

Nieistniejące rozpoznanie → 404 :
curl -s -i -X PATCH "http://localhost:5000/diagnoses/00000000-0000-0000-0000-000000000000?newStatus=Cured"

Brak nowego statusu → 400 :
curl -s -i -X PATCH "http://localhost:5000/diagnoses/52486ffa-9e0e-4bbb-be7c-4a956e9130df"

Poprawna zmiana (Active → Cured) → 200 (zwraca rozpoznanie po aktualizacji):
curl -s -i -X PATCH "http://localhost:5000/diagnoses/52486ffa-9e0e-4bbb-be7c-4a956e9130df?newStatus=Cured"

Zabroniona zmiana (z Cured → na Active) → 409 (konflikt):
curl -s -i -X PATCH "http://localhost:5000/diagnoses/52486ffa-9e0e-4bbb-be7c-4a956e9130df?newStatus=Active"


