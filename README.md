## Założenia projektowe

**Pacjenci są danymi startowymi (seed), nie tworzeni przez endpoint rejestracji rozpoznania.**

Treść zadania (`ZADANIE.md`, sekcja "Dane") mówi: 
*"Przygotuj dane startowe sam. Muszą się w nich znaleźć: dwóch różnych pacjentów o tym samym numerze kartoteki, jeden z SYS-A, drugi z SYS-B..."* — to wskazuje, że pacjenci mają istnieć w bazie **z góry**, przed jakąkolwiek rejestracją rozpoznania, a nie być tworzeni "przy okazji" pierwszego zgłoszenia.

Trzy reguły walidacji przy rejestracji rozpoznania (sekcja "Rejestracja rozpoznania") dotyczą wyłącznie:
- daty postawienia, 
- kodu ICD-10 
- opisu słownego — zadanie nie wymaga tworzenia nowego pacjenta jako część tej operacji.

Dodatkowo, poza tymi trzema, dołożona jest jeszcze jedna, własna reguła: **pacjent musi istnieć w bazie** (po PESEL albo numerze kartoteki+systemie).
Zadanie tego wprost nie wymienia wśród "trzech rzeczy", ale bez tego rejestracja rozpoznania nie miałaby komu przypisać wyniku — więc to sensowne
rozszerzenie, nie coś sprzecznego z treścią zadania. Przy okazji ta sama reguła załatwia bez dodatkowego kodu przypadek "brak PESEL i numeru
kartoteki naraz" — skoro żadne z dwóch nie jest podane, żaden pacjent i tak nie zostanie znaleziony.

**Konsekwencja:** `POST /diagnoses` identyfikuje pacjenta po numerze PESEL albo (numer kartoteki + system źródłowy SYS-A/SYS-B), zakładając że taki pacjent już istnieje w bazie (wstawiony jako dane startowe). Endpoint nie przyjmuje imienia/nazwiska/daty urodzenia pacjenta.

## Błędy które testuje w `data.json`

Na razie sprawdzane są dokładnie te przypadki, które zadanie wprost wymienia:
- data postawienia rozpoznania z przyszłości
- kod ICD-10, którego nie ma w zbiorze
- kod wycofany (`Z00`)
- kod podany z obcym systemem kodowania
- początek dolegliwości: podane oba naraz (data i wiek) — zabronione wprost w treści zadania
- początek dolegliwości: nie podane żadne z dwóch
- to samo rozpoznanie wysłane drugi raz (ma się udać, nie zwrócić błędu — test idempotencji)
- kilka naruszeń naraz: jeden przypadek z 2 błędami (data + kod), jeden z maksymalną liczbą naraz (4: data + kod + system + oba pola początku) — sprawdza, czy walidacja zbiera WSZYSTKIE błędy naraz, nie tylko pierwszy napotkany
- pacjent nie istnieje: raz przez błędny PESEL, raz przez błędny numer zewnętrzny kartoteki

## Błedy które jeszcze można by kiedyś testować (na razie odłożone)

- brak externalSystemKind albo inny, nieobsługiwany

## testowanie
test 1 - dobre dane:
request: 
    curl -i -X POST http://localhost:5000/diagnoses -H "Content-Type: application/json" -d @data-test-good.json
responce: 
    "7) Nowa  diagnoza z sytemu: SysA, o symbolu: REC-A-001 200"

test 2 - złe dane:
request: 
    curl -i -X POST http://localhost:5000/diagnoses -H "Content-Type: application/json" -d @data-test-bad.json 
responce: 
    {"type":"https://tools.ietf.org/html/rfc9110#section-15.5.1","title":"One or more validation errors occurred.","status":400,"errors":{"REC-TEST-002":["Data diagnozy nie może być z przyszłości: 01.01.2030","Kod Icd10: XYZ99, błędny lub nieaktywny"]}}
