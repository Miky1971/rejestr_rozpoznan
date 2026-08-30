# Zadanie końcowe: rejestr rozpoznań

## O co chodzi

Przychodnia rejestruje rozpoznania stawiane pacjentom. Rozpoznanie ma kod ICD-10 razem
z systemem kodowania i opisem słownym, datę postawienia, status kliniczny (aktywne, wyleczone,
nawrót) oraz stopień potwierdzenia (podejrzenie, potwierdzone). Początek dolegliwości podaje
się albo jako datę, albo jako wiek pacjenta, nigdy jako jedno i drugie. Rozpoznanie dotyczy
jednego pacjenta i może wskazywać wizytę, w czasie której powstało.

Dane płyną z dwóch systemów, SYS-A i SYS-B. Każdy nadaje własny identyfikator rekordu.
Pacjenta rozpoznajemy po numerze PESEL albo po numerze kartoteki. Ten sam numer kartoteki
występuje w obu systemach i oznacza wtedy dwie różne osoby.

## Rejestracja rozpoznania

Zbuduj operację REST, która przyjmuje rozpoznanie razem z identyfikatorem systemu źródłowego.
Udana rejestracja zwraca 201 i nagłówek Location.

Zanim zapiszesz, sprawdź trzy rzeczy. Data postawienia nie może być z przyszłości. Kod ICD-10 musi znajdować się w zbiorze dopuszczalnych kodów, pochodzić z właściwego systemu kodowania. Opis słowny zapisujesz ze zbioru, nie z danych wejściowych, bo wejście może kłamać.

Gdy któraś reguła jest naruszona, zwróć 400 w formacie application/problem+json i wypisz
wszystkie naruszenia naraz, nie pierwsze napotkane. Treść wyjątku ani stack trace nie mogą
trafić do klienta. Odmowa jest zwykłym wynikiem operacji, nie wyjątkiem: warstwa HTTP tylko
tłumaczy ten wynik na kod odpowiedzi.

Ten sam rekord potrafi przyjść dwa razy z tym samym identyfikatorem źródłowym. Drugie przyjście
nie może utworzyć kopii ani zakończyć się błędem serwera.

## Zbiór dopuszczalnych kodów

Kody opisz statycznym zasobem ValueSet i wczytaj go z pliku JSON przy starcie aplikacji.
Strukturę zasobu znajdziesz w dokumentacji. Lista kodów wpisana w kod C# nie jest rozwiązaniem
tego punktu.

Zbiór ma własny adres kanoniczny, jawnie wskazuje system kodowania, zawiera co najmniej pięć kodów z opisami i pozwala sprawdzić kod bez dostępu do sieci.

## Zgłoszenie do rejestru sprawozdawczego

Po udanej rejestracji rozpoznanie trafia do zewnętrznego rejestru. Rejestru nie ma, więc
napisz atrapę: osobny endpoint wołany po HTTP, z licznikiem prób prowadzonym osobno dla
każdego rozpoznania. Pierwsze dwie próby odrzuca kodem 503, trzecia i kolejne przyjmują
zgłoszenie kodem 202. Zgłoszenie bez identyfikatora rozpoznania albo bez kodu odrzuca kodem
400, natychmiast i zawsze.

Twoja aplikacja rozmawia z atrapą po HTTP i nie wolno jej obchodzić. Zgłoszenie ponawiaj najwyżej trzy razy, z przerwą między próbami. Ponawiaj wyłącznie błędy przejściowe. Odrzucenie z powodu treści zgłoszenia przejściowe nie jest. Walidacja kodu ICD-10 działa lokalnie i nie ma tam czego ponawiać.

Gdy rejestr milczy po trzech próbach, rozpoznanie zostaje zapisane. Nieudane zgłoszenie nie
może zniknąć po cichu: jego stan musi być widoczny przez API.

## Odczyt

Wystaw odczyt rozpoznań wskazanego pacjenta z filtrem po statusie klinicznym i sortowaniem
po dacie postawienia. Wystaw też wyszukanie pacjenta po numerze PESEL albo po numerze
kartoteki; wynik musi być jednoznaczny również wtedy, gdy ten numer kartoteki istnieje
w obu systemach.

Każda lista jest stronicowana i ma twardy górny limit rozmiaru strony, którego klient nie
przeskoczy.

## Zestawienie

Policz liczbę rozpoznań w podziale na kod ICD-10, malejąco. To zapytanie napisz w SQL
i wykonaj Dapperem. Nie licz go w pamięci procesu ani przez EF.

## Zmiana statusu

Pozwól zmienić status kliniczny rozpoznania. Przejście z wyleczone na aktywne jest zabronione
i kończy się kodem 409.

## GraphQL

Ten sam model wystaw dodatkowo pod jednym endpointem GraphQL. Potrzebne są co najmniej jedno  
zapytanie z zagnieżdżeniem, które prowadzi od pacjenta przez jego rozpoznania do kodu,  
oraz jedna mutacja, która przy odmowie oddaje błąd w danych odpowiedzi, a nie wyjątkiem.

## Baza

Zapisy przez EF Core na SQLite. Schemat zakładany migracją, nie EnsureCreated. Powiązane
wiersze zapisywane w jednej transakcji.

## Dane

Przygotuj dane startowe sam. Muszą się w nich znaleźć: dwóch różnych pacjentów o tym samym  
numerze kartoteki, jeden z SYS-A, drugi z SYS-B; ten sam rekord rozpoznania przysłany dwa  
razy; data postawienia z przyszłości; kod nieobecny w zbiorze; kod wycofany; kod podany  
z obcym systemem kodowania; rozpoznanie z początkiem podanym jako wiek i drugie z początkiem  
podanym jako data; rozpoznanie wyleczone, na którym spróbujesz zabronionego przejścia.

