## Założenia projektowe

**Pacjenci są danymi startowymi (seed), nie tworzeni przez endpoint rejestracji rozpoznania.**

Treść zadania (`ZADANIE.md`, sekcja "Dane") mówi: 
*"Przygotuj dane startowe sam. Muszą się w nich znaleźć: dwóch różnych pacjentów o tym samym numerze kartoteki, jeden z SYS-A, drugi z SYS-B..."* — to wskazuje, że pacjenci mają istnieć w bazie **z góry**, przed jakąkolwiek rejestracją rozpoznania, a nie być tworzeni "przy okazji" pierwszego zgłoszenia.

Trzy reguły walidacji przy rejestracji rozpoznania (sekcja "Rejestracja rozpoznania") dotyczą wyłącznie:
- daty postawienia, 
- kodu ICD-10 
- opisu słownego — zadanie nie wymaga tworzenia nowego pacjenta jako część tej operacji.

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

## Błedy które jeszcze można by kiedyś testować (na razie odłożone)

- identyfikacja pacjenta, brak obu naraz (PESEL / numer zewnętrznej kartoteki)
- brak systemKind lub inny nie obsługiwany
