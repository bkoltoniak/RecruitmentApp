# Opis zadania
Architektura:
zastosowana architektura aplikacji to N-Tier architecture. Aplikacja składa się z trzech warstw: dostępu do danych(data), biznesowej(core) i interfejsu API(webapi). Jest to nieskomplikowana architektura zbudowana w oparciu o koncepcje dependency inversion oraz separation of concerns. Warstwy maja ściśle określone zasady dotyczące zależności, co ułatwi późniejszy rozwój i utrzymanie. 

Authentykacja:
W aplikacji została zaimplementowana za pomocą algorytmu HMAC.
Treść klucza zawiera informacje o dacie wygenerowania, używaną do walidacji maksymalnej dopuszczalnej długości życia. Moduł autoryzacyjny został wyenkapsułowany do filtra asp.net core, w celu zdjęcia tej odpowiedzialności z kontrolera.

Moduł pobierania walut: Pobieranie walut odbywa się za pomocą API Europejskiego Banku Centralnego. ECB zapewnia kursy walut w określonych walutach w stosunku do euro. Po stronie aplikacji wyliczane są kursy walut w stosunku do innych walut wspieranych przez API ECB. Api zwraca kursy walut z przeszłości. W dni kiedy kurs walut nie było dostępny, zostaje zwrócony ostatni dostępny kurs. 

Optymalizacje: Zastosowane zostało cachowanie wyników odpowiedzi ASP.Net oraz cachowanie zapytań o kursy walut do API po stronie bazy danych. Zastosowana baza to MSSQL.


