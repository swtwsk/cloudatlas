# Andrzej Swatowski - CloudAtlas

Andrzej Swatowski, as386085

Pierwsze zadanie zaliczeniowe z przedmiotu "Systemy rozproszone"

## Projekt
### Budowa
Backend projektu (interpreter, serwer oraz aplikacje klienckie) jest napisany w języku C#, frontend w JavaScripcie z pomocą frameworku VueJS.

Projekt korzysta z open source'owej dystrybucji .NET Core, która powinna działać nie tylko na Windowsie, ale także na Linuksie oraz systemach OS X. Niestety, wersja .NET Core zainstalowana na `students` nie działa poprawnie - `dotnet` wyrzuca błąd o braku odpowiedniej wersji OpenSSL. Z tego co rozumiem, związane jest to z zainstalowaną tam, starszą wersją .NET Core'a, bowiem odpowiadające temu błędowi issue na GitHubie są oznaczone jako rozwiązane w nowszych wersjach `dotnet`, postaram się skontaktować w przyszłym tygodniu z administratorem `students` z prośbą o zaktualizowanie programu, mam nadzieję, że rozwiąże to problemy z niekompatybilnością.

.NET Core poprawnie działa jednak chociażby na Ubuntu (projekt był pisany na Ubuntu 16.04 oraz Mac OS X Sierra), wystarczy zainstalować go zgodnie z instrukcją ze strony [dotnet](https://dotnet.microsoft.com/download/linux-package-manager/sdk-current).

Po zainstalowaniu `dotnet` zbudowanie projektu wymaga wpisania komendy:
```
dotnet build
```
w głównym katalogu projektu.

### Struktura i uruchomienie

Projekt podzielony jest na cztery aplikacje, z których każdą najłatwiej uruchomić wewnątrz odpowiadającego jej katalogu:
 - **Interpreter** jest samodzielną aplikacją konsolową, służącą za interpreter zapytań stylizowanych na SQL. Uruchomienie (`zmiFileName` jest opcjonalnym argumentem, oznaczającym ścieżkę do pliku opisującego wszystkie ZMI; domyślnie interpreter szuka pliku `zmis.txt`):
 ```bash
 cd Interpreter
 dotnet run [zmiFileName]
 ```
 
 - **CloudAtlasAgent** jest serwerem i wrapperem na interpreter. Domyślnie uruchamiany jest pod adresem `127.0.0.1:5000` i wyszukuje opisującego wszystkie ZMI pliku `zmis.txt`, ale można to zmienić odpowiednimi argumentami. Opis poszczególnych opcji wyświetli się, gdy po wejściu do katalogu wpiszemy komendę `dotnet run -- --help`. Standardowe uruchomienie:
 ```bash
 cd CloudAtlasAgent
 dotnet run
 ```

 - **CloudAtlasClient** jest aplikacją kliencką, która komunikuje się z **Agentem** za pomocą wywołań RPC i służy za serwer HTTP wystawiający interfejs użytkownika przez stronę WWW. Podobnie, jak w przypadku **Agenta** możemy wyświetlić dodatkowe opcje uruchamiania za pomocą komendy `dotnet run -- --help`. Opcje pozwalają ustawić inny niż domyślny adres i port serwera oraz adres i port endpointu WWW klienta.
 **Client** korzysta z inspirowanego Sinatrą i Flaskiem miniframeworku Nancy, by serwować stronę WWW i wystawiać REST API z danymi pobieranymi z **Agenta**. Strona internetowa napisana została za pomocą frameworku VueJS, jej źródła znajdują się w katalogu `FrontEnd` (podkatalog korzenia projektu), jednakże z racji tego, że wykorzystują do kompilacji NPMa postanowiłem ostatnią, skompilowaną już wersję umieścić bezpośrednio w folderach **Clienta**, w folderze `dist`. Frontend co 10 sekund wysyła RESTowe zapytanie do **Clienta**, by aktualizować swoje dane (a w szczególności wykresy).
 ```bash
 cd CloudAtlasClient
 dotnet run
 ```

 - **Fetcher** jest drugą aplikacją kliencką, który aktualizuje na bieżąco dane o maszynie na której działa poprzez uruchomienie bashowego skryptu `fetch.sh`. Podobnie, jak poprzednie aplikacje pozwala na wyświetlenie pomocy z opisem opcji za pomocą `dotnet run -- --help`. Do uruchomienia potrzebuje podania ścieżki do pliku inicjalizacyjnego, np.:
 ```bash
 cd Fetcher
 dotnet run -i sample.ini
 ```

Oprócz tego część plików projektu znajduje się w katalogu `Shared`. Umieszczone tam zostały pliki Modelu CloudAtlasa, deklaracja metod możliwych do wywoływania przez RPC oraz liczne klasy i funkcje pomocnicze, które są wykorzystywane przez wszystkie aplikacje.

