# Andrzej Swatowski - CloudAtlas

Andrzej Swatowski, as386085

Pierwsze zadanie zaliczeniowe z przedmiotu "Systemy rozproszone"

## Projekt
### Uruchomienie
Backend projektu (interpreter, serwer oraz aplikacje klienckie) jest napisany w języku C#, frontend w JavaScripcie z pomocą frameworku VueJS.

Projekt korzysta z open source'owej dystrybucji .NET Core, która powinna działać nie tylko na Windowsie, ale także na Linuksie oraz systemach OS X. Niestety, wersja .NET Core zainstalowana na `students` nie działa poprawnie - `dotnet` wyrzuca błąd o braku odpowiedniej wersji OpenSSL. Z tego co rozumiem, związane jest to z zainstalowaną tam, starszą wersją .NET Core'a, bowiem odpowiadające temu błędowi issue na GitHubie są oznaczone jako rozwiązane w nowszych wersjach `dotnet`, postaram się skontaktować w przyszłym tygodniu z administratorem `students` z prośbą o zaktualizowanie programu, mam nadzieję, że rozwiąże to problemy z niekompatybilnością.

.NET Core poprawnie działa jednak chociażby na Ubuntu (projekt był pisany na Ubuntu 16.04 oraz Mac OS X Sierra), wystarczy zainstalować go zgodnie z instrukcją ze strony [dotnet](https://dotnet.microsoft.com/download/linux-package-manager/sdk-current).

