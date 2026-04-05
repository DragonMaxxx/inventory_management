# Trisecmed

System do ewidencji i zarządzania aparaturą medyczną w szpitalu. Powstał jako projekt inżynierski — docelowo ma obsługiwać cały cykl życia urządzenia medycznego: od przyjęcia na stan, przez przeglądy techniczne i awarie, aż po kasację.

Na ten moment zaimplementowany jest backend (REST API). Frontend będzie dodany po ukończeniu backendu.

## O co chodzi w tym projekcie

W szpitalach jest mnóstwo sprzętu medycznego — aparaty RTG, respiratory, pompy infuzyjne, defibrylatory. Każde urządzenie musi mieć aktualny przegląd techniczny, ktoś musi pilnować gwarancji, a jak się zepsuje — trzeba to zgłosić, przypisać serwisanta i śledzić naprawę. Do tego dochodzi rozliczanie kosztów, raporty dla kierownictwa i wymagania prawne (RODO, audyt).

Trisecmed to system, który zbiera to wszystko w jedno miejsce. Zamiast Exceli, papierowych kart i telefonów — jedno API, do którego można podpiąć dowolny frontend.

## Stos technologiczny

- **C# / .NET 10 / ASP.NET Core** — główny framework
- **Entity Framework Core** — ORM, migracje bazy danych
- **PostgreSQL 16** — baza danych
- **MediatR** — wzorzec CQRS (oddzielenie komend od zapytań)
- **FluentValidation** — walidacja danych wejściowych
- **JWT (JSON Web Tokens)** — uwierzytelnianie
- **BCrypt** — hashowanie haseł
- **Serilog** — logowanie
- **Docker + Nginx** — konteneryzacja i reverse proxy
- **xUnit + FluentAssertions** — testy
- **ClosedXML** — import z Excela
- **Hangfire** — kolejka zadań (przyszłościowo: powiadomienia, backupy)

## Jak to uruchomić

Potrzebujesz Docker Desktop (uruchomiony) i Git.

```bash
cd inventory_management

# Skopiuj plik konfiguracyjny
cp .env.example .env

# Uruchom
docker compose up --build -d
```

Po chwili API jest dostępne na `http://localhost:5000`. Żeby sprawdzić czy działa:

```bash
curl http://localhost:5000/health
```

Powinno zwrócić `Healthy`. Dokumentacja API (interaktywna) jest pod `http://localhost:5000/scalar/v1`.

Żeby zatrzymać:

```bash
docker compose down        # zatrzymaj
docker compose down -v     # zatrzymaj i wyczyść bazę (reset)
```

## Konta w trybie dev

Przy pierwszym uruchomieniu system tworzy trzy konta z różnymi rolami:

| Email | Hasło | Rola |
|-------|-------|------|
| admin@trisecmed.local | Admin123 | Administrator |
| worker@trisecmed.local | Worker123 | EquipmentWorker |
| manager@trisecmed.local | Manager123 | EquipmentManager |

Każda rola ma inne uprawnienia — szczegóły w sekcji "Role i uprawnienia".

## Jak działa uwierzytelnianie

System używa tokenów JWT. Żeby cokolwiek zrobić (poza health checkiem), trzeba się najpierw zalogować:

1. Wysyłasz email + hasło na `/api/v1/auth/login`
2. Dostajesz `accessToken` (ważny 15 minut) i `refreshToken` (ważny 7 dni)
3. Przy każdym kolejnym zapytaniu dołączasz `accessToken` w nagłówku `Authorization: Bearer <token>`
4. Jak access token wygaśnie — odświeżasz go przez `/api/v1/auth/refresh`

## Co aktualnie działa

### Moduł Tożsamość (Identity)

Logowanie, wylogowanie, odświeżanie tokenów, tworzenie kont przez admina, aktywacja konta tokenem. Każda operacja modyfikacji danych jest automatycznie zapisywana w audit logu (kto, co, kiedy zmienił).

### Moduł Aparatura (Equipment)

To główny moduł systemu. Obsługuje:

- **Lista urządzeń** z filtrowaniem (po statusie, oddziale, kategorii), wyszukiwaniem tekstowym, sortowaniem i paginacją
- **Szczegóły urządzenia** — wszystkie dane łącznie z nazwą oddziału i kategorii
- **Dodawanie i edycja** urządzeń z walidacją (np. unikalny numer inwentarzowy, wymagane pola)
- **Zmiana statusu** (Active, InRepair, Archived, Disposed) — tylko kierownik DAM
- **Kasacja** (soft-delete — urządzenie dostaje status Archived, nie jest usuwane z bazy)
- **Przeglądy techniczne** — dodawanie wpisów o przeglądach, historia przeglądów urządzenia, automatyczna aktualizacja daty następnego przeglądu
- **Historia awarii** — podgląd awarii przypisanych do urządzenia
- **Import z Excela** — wgrywanie urządzeń z pliku .xlsx (mapowanie kolumn po nazwie, raport błędów i duplikatów)

## Endpointy API

### Uwierzytelnianie — `/api/v1/auth`

| Metoda | Endpoint | Opis |
|--------|----------|------|
| POST | /auth/login | Logowanie |
| POST | /auth/refresh | Odświeżenie tokenu |
| POST | /auth/logout | Wylogowanie |
| POST | /auth/activate | Aktywacja konta |

### Użytkownicy — `/api/v1/users` (tylko admin)

| Metoda | Endpoint | Opis |
|--------|----------|------|
| GET | /users | Lista użytkowników |
| POST | /users | Utworzenie konta |

### Urządzenia — `/api/v1/devices`

| Metoda | Endpoint | Kto może | Opis |
|--------|----------|----------|------|
| GET | /devices | worker, manager, admin | Lista (filtrowanie, sortowanie, paginacja) |
| GET | /devices/{id} | wszyscy zalogowani | Szczegóły urządzenia |
| GET | /devices/department/{deptId} | wszyscy zalogowani | Urządzenia z danego oddziału |
| POST | /devices | worker, manager | Dodanie urządzenia |
| PUT | /devices/{id} | worker, manager | Edycja urządzenia |
| PATCH | /devices/{id}/status | manager | Zmiana statusu |
| DELETE | /devices/{id} | manager | Kasacja (soft-delete) |
| GET | /devices/{id}/inspections | worker, manager | Historia przeglądów |
| POST | /devices/{id}/inspections | worker, manager | Nowy wpis przeglądu |
| GET | /devices/{id}/failures | worker, manager | Historia awarii |
| POST | /devices/import | admin | Import z pliku Excel |

**Filtrowanie listy urządzeń:**
- `?page=1&pageSize=25` — paginacja (domyślnie 25, max 100)
- `?sortBy=name&sortDir=desc` — sortowanie (name, inventorynumber, manufacturer, status, purchasedate, createdat)
- `?status=Active` — filtr po statusie (Active, InRepair, Archived, Disposed)
- `?departmentId=11111111-...` — filtr po oddziale
- `?categoryId=22222222-...` — filtr po kategorii
- `?search=RTG` — szukaj w nazwie, numerze inwentarzowym, producencie, modelu

## Role i uprawnienia

System ma cztery role. Każda rola widzi i może robić inne rzeczy:

**Nurse (Pielęgniarka)** — widzi urządzenia swojego oddziału, może zgłaszać awarie. Nie może dodawać ani edytować urządzeń.

**EquipmentWorker (Pracownik DAM)** — widzi wszystkie urządzenia, może je dodawać, edytować, dodawać przeglądy, obsługiwać awarie. Nie może kasować urządzeń ani zmieniać statusu na "disposed".

**EquipmentManager (Kierownik DAM)** — wszystko co worker, plus może kasować urządzenia (soft-delete) i zmieniać ich status.

**Administrator** — zarządza kontami użytkowników, importuje dane z Excela. Nie operuje bezpośrednio na urządzeniach (od tego jest worker/manager).

## Dane testowe (seed)

Przy starcie w trybie dev system automatycznie tworzy:

**3 oddziały:** Chirurgia (CHIR), Kardiologia (KARD), Intensywna Terapia (OIT)

**4 kategorie urządzeń:** Aparatura diagnostyczna, Aparatura terapeutyczna, Sprzęt laboratoryjny, Wyposażenie pomocnicze

**Stałe ID do testów:**
- Oddział Chirurgii: `11111111-1111-1111-1111-111111111111`
- Aparatura diagnostyczna: `22222222-2222-2222-2222-222222222222`
- Admin: `33333333-3333-3333-3333-333333333333`
- Worker: `44444444-4444-4444-4444-444444444444`
- Manager: `55555555-5555-5555-5555-555555555555`

## Architektura

Projekt jest zbudowany w Clean Architecture — cztery warstwy, zależności płyną do wewnątrz:

```
backend/
├── src/
│   ├── Trisecmed.Domain/           # Encje, enumy, interfejsy — zero zależności
│   ├── Trisecmed.Application/      # Logika biznesowa — komendy, zapytania, walidacja
│   ├── Trisecmed.Infrastructure/   # Baza danych, repozytoria, JWT, BCrypt, Excel
│   └── Trisecmed.API/              # Kontrolery HTTP, middleware, konfiguracja
├── tests/
│   ├── Trisecmed.Unit.Tests/       # Testy jednostkowe
│   └── Trisecmed.Integration.Tests/ # Testy integracyjne
└── Dockerfile
```

**Domain** nie zależy od niczego — to czyste klasy C# opisujące co system przechowuje (Device, User, Failure, Inspection...).

**Application** zawiera logikę biznesową. Każda operacja to osobna klasa — np. `CreateDeviceCommand` + `CreateDeviceHandler`. Walidacja jest w osobnych klasach (FluentValidation). Application zależy tylko od Domain.

**Infrastructure** to implementacja techniczna — Entity Framework, repozytoria, JWT, BCrypt. Zależy od Application i Domain.

**API** to warstwa wejściowa — kontrolery REST, middleware, konfiguracja DI. Zależy od wszystkich.

## Baza danych

PostgreSQL 16, dostępna na `localhost:5432` (user: `trisecmed_user`, password: `dev_password_123`, baza: `trisecmed`). Można podłączyć się pgAdminem, DBeaverem albo psqlem.

Migracje są automatyczne przy starcie w trybie dev. Żeby dodać nową migrację ręcznie:

```bash
cd backend
dotnet ef migrations add NazwaMigracji \
  --project src/Trisecmed.Infrastructure \
  --startup-project src/Trisecmed.API \
  --output-dir Data/Migrations \
  --configuration Release
```

## Testy automatyczne

```bash
cd backend
dotnet test --configuration Release
```

Aktualnie jest 32 testów: 9 domain, 19 application (walidatory, Result, PagedResult), 4 integracyjne.

## CI/CD

GitHub Actions (`.github/workflows/ci.yml`) — przy każdym pushu buduje projekt, uruchamia testy z prawdziwą bazą PostgreSQL i sprawdza czy Docker build przechodzi.

## Porty

| Usługa | Port |
|--------|------|
| API (Docker) | 5000 |
| API (lokalne) | 5008 |
| PostgreSQL | 5432 |
| Nginx | 80 |

## Dokumentacja

- `docs/Trivaxa_Specyfikacja_Backend.pdf` — pełna specyfikacja techniczna backendu
- `docs/testy/` — scenariusze testowe do ręcznego testowania systemu

## Stan implementacji

| Faza | Co robi | Status |
|------|---------|--------|
| 0 - Setup | Struktura, Docker, CI/CD, migracje, health check | Gotowe |
| 1 - Tożsamość | Logowanie JWT, role RBAC, audit log | Gotowe |
| 2 - Aparatura | Pełny CRUD urządzeń, przeglądy, import Excel | Gotowe |
| 3 - Awarie | Zgłoszenia, statusy, historia, koszty napraw | Do zrobienia |
| 4 - Powiadomienia | Przypomnienia o przeglądach, email, Hangfire | Do zrobienia |
| 5 - Raporty | Generowanie PDF i Excel, eksport RODO | Do zrobienia |
| 6 - Serwisanci | Firmy serwisowe, umowy, słowniki | Do zrobienia |
| 7 - Testy | Testy wydajnościowe, penetracyjne, audyt | Do zrobienia |

## Autorzy

- mgr inż. Mateusz Bartoszewicz
- mgr inż. Stanisław Rachwał
