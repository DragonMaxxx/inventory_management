# Trisecmed

System do ewidencji i zarządzania aparaturą medyczną w szpitalu. Obsługuje cały cykl życia urządzenia medycznego: od przyjęcia na stan, przez przeglądy techniczne i awarie, aż po kasację.

Składa się z **backendu** (REST API w .NET 10) i **frontendu** (Blazor WebAssembly z MudBlazor).

## Szybki start

### Wymagania

- **Docker Desktop** (uruchomiony)
- **.NET 10 SDK** (do frontendu i developmentu)
- **Git**

### Uruchomienie całego systemu

```bash
# 1. Sklonuj repozytorium i wejdź do katalogu
cd inventory_management

# 2. Uruchom backend + bazę danych na Dockerze
docker compose up --build -d

# 3. Sprawdź czy backend działa
curl http://localhost:5000/health
# Powinno zwrócić: Healthy

# 4. Uruchom frontend (w osobnym terminalu)
cd frontend
dotnet run --urls "http://localhost:5002"
```

Po uruchomieniu:

| Usługa | Adres |
|--------|-------|
| **Frontend (Blazor)** | http://localhost:5002 |
| **Backend API** | http://localhost:5000 |
| **Dokumentacja API (Scalar)** | http://localhost:5000/scalar/v1 |
| **Panel Hangfire (joby)** | http://localhost:5000/hangfire |
| **PostgreSQL** | localhost:5432 |

### Zatrzymanie

```bash
# Zatrzymaj backend + bazę
docker compose down

# Zatrzymaj i wyczyść bazę (pełny reset)
docker compose down -v

# Frontend — Ctrl+C w terminalu gdzie uruchomiłeś dotnet run
```

## Konta testowe

Przy pierwszym uruchomieniu system tworzy trzy konta z różnymi rolami. **Każda rola widzi inne elementy interfejsu** — przyciski i strony są ukrywane zgodnie z uprawnieniami.

| Email | Hasło | Rola | Do czego służy |
|-------|-------|------|----------------|
| `admin@trisecmed.local` | `Admin123` | Administrator | Zarządzanie użytkownikami, import Excel, GDPR, słowniki |
| `worker@trisecmed.local` | `Worker123` | EquipmentWorker | Dodawanie urządzeń, przeglądy, obsługa awarii, raporty |
| `manager@trisecmed.local` | `Manager123` | EquipmentManager | Wszystko co worker + usuwanie urządzeń, zmiana statusów |

**Aby przetestować dodawanie urządzeń i awarii** — zaloguj się jako `worker` lub `manager`. Admin nie ma do tego uprawnień (celowo — admin zarządza systemem, nie operuje na sprzęcie).

## Co robi system

### Moduły backendu

| Moduł | Opis |
|-------|------|
| **Tożsamość** | Logowanie JWT, odświeżanie tokenów, tworzenie kont, aktywacja, role RBAC, audit log |
| **Aparatura** | CRUD urządzeń, przeglądy techniczne, historia awarii, import z Excela |
| **Awarie** | Zgłaszanie awarii, zmiana statusów, przypisanie serwisanta, rozwiązanie z kosztem naprawy, historia zmian |
| **Powiadomienia** | Automatyczne emaile: zbliżający się przegląd (30 dni), wygasająca gwarancja (14 dni), nowa awaria |
| **Raporty** | Generowanie PDF i Excel (urządzenia, awarie, przeglądy), eksport danych GDPR |
| **Słowniki** | Kategorie urządzeń, oddziały, firmy serwisowe |

### Strony frontendu

| Strona | URL | Kto widzi | Co można robić |
|--------|-----|-----------|---------------|
| Dashboard | `/` | Wszyscy | Statystyki: urządzenia, otwarte awarie, kategorie, oddziały |
| Urządzenia | `/devices` | Wszyscy | Lista, filtrowanie, szukanie. Worker/Manager: dodaj, edytuj. Manager: archiwizuj. Admin: import Excel |
| Szczegóły urządzenia | `/devices/{id}` | Wszyscy | Dane urządzenia, przeglądy (lista + dodawanie), zmiana statusu (Manager) |
| Awarie | `/failures` | Wszyscy | Lista, filtrowanie. Nurse/Worker/Manager: zgłoś awarię |
| Szczegóły awarii | `/failures/{id}` | Wszyscy | Szczegóły, zmiana statusu, przypisanie serwisanta, rozwiązanie, historia zmian |
| Kategorie | `/categories` | Wszyscy | Lista. Manager/Admin: dodaj, edytuj. Admin: usuń |
| Oddziały | `/departments` | Wszyscy | Lista. Manager/Admin: dodaj, edytuj. Admin: usuń |
| Serwisanci | `/service-providers` | Worker+ | Lista. Manager/Admin: dodaj, edytuj. Admin: usuń |
| Raporty | `/reports` | Worker+ | Pobieranie raportów PDF/Excel. Admin: eksport GDPR |
| Powiadomienia | `/notifications` | Worker+ | Lista wysłanych powiadomień email |
| Użytkownicy | `/users` | Admin | Lista użytkowników, tworzenie nowych kont |

## Role i uprawnienia

| Akcja | Nurse | Worker | Manager | Admin |
|-------|:-----:|:------:|:-------:|:-----:|
| Przeglądanie urządzeń | + | + | + | + |
| Dodawanie/edycja urządzeń | - | + | + | - |
| Zmiana statusu urządzenia | - | - | + | - |
| Archiwizacja urządzenia | - | - | + | - |
| Dodawanie przeglądów | - | + | + | - |
| Zgłaszanie awarii | + | + | + | - |
| Obsługa awarii (status/serwisant/rozwiązanie) | - | + | + | - |
| Tworzenie/edycja kategorii i oddziałów | - | - | + | + |
| Usuwanie kategorii i oddziałów | - | - | - | + |
| Tworzenie/edycja serwisantów | - | - | + | + |
| Pobieranie raportów (PDF/Excel) | - | + | + | - |
| Eksport GDPR | - | - | - | + |
| Import urządzeń z Excela | - | - | - | + |
| Zarządzanie użytkownikami | - | - | - | + |

## Stos technologiczny

### Backend
- **C# / .NET 10 / ASP.NET Core** — framework
- **Entity Framework Core** — ORM, migracje
- **PostgreSQL 16** — baza danych
- **MediatR** — wzorzec CQRS
- **FluentValidation** — walidacja
- **JWT** — uwierzytelnianie
- **BCrypt** — hashowanie haseł
- **Hangfire** — kolejka zadań (cron joby)
- **MailKit** — wysyłanie emaili SMTP
- **QuestPDF** — generowanie raportów PDF
- **ClosedXML** — generowanie/import plików Excel
- **Serilog** — logowanie
- **Docker + Nginx** — konteneryzacja

### Frontend
- **Blazor WebAssembly** — SPA w C#
- **MudBlazor 9.2** — biblioteka komponentów Material Design
- **JS Interop** — localStorage dla tokenów JWT

## Architektura

```
inventory_management/
├── backend/
│   ├── src/
│   │   ├── Trisecmed.Domain/           # Encje, enumy, interfejsy — zero zależności
│   │   ├── Trisecmed.Application/      # Logika biznesowa — komendy, zapytania, walidacja
│   │   ├── Trisecmed.Infrastructure/   # EF Core, repozytoria, JWT, email, raporty, joby
│   │   └── Trisecmed.API/              # Kontrolery REST, middleware, konfiguracja
│   ├── tests/
│   │   ├── Trisecmed.Unit.Tests/       # Testy jednostkowe
│   │   └── Trisecmed.Integration.Tests/ # Testy integracyjne
│   └── Dockerfile
├── frontend/
│   ├── Pages/                           # Strony Blazor (.razor)
│   ├── Services/                        # AuthService, ApiClient
│   ├── Models/                          # DTO i modele formularzy
│   ├── Layout/                          # MainLayout z nawigacją
│   └── wwwroot/                         # index.html, appsettings.json
├── docker-compose.yml
└── README.md
```

Backend zbudowany w **Clean Architecture** — zależności płyną do wewnątrz (Domain ← Application ← Infrastructure ← API).

## Endpointy API

### Uwierzytelnianie — `/api/v1/auth`

| Metoda | Endpoint | Opis |
|--------|----------|------|
| POST | /login | Logowanie (zwraca accessToken + refreshToken) |
| POST | /refresh | Odświeżenie tokenu |
| POST | /logout | Wylogowanie |
| POST | /activate | Aktywacja konta tokenem |

### Użytkownicy — `/api/v1/users` (admin)

| Metoda | Endpoint | Opis |
|--------|----------|------|
| GET | / | Lista użytkowników |
| POST | / | Utworzenie konta |

### Urządzenia — `/api/v1/devices`

| Metoda | Endpoint | Kto może | Opis |
|--------|----------|----------|------|
| GET | / | worker, manager, admin | Lista z filtrowaniem, sortowaniem, paginacją |
| GET | /{id} | wszyscy zalogowani | Szczegóły urządzenia |
| GET | /department/{deptId} | wszyscy zalogowani | Urządzenia z oddziału |
| POST | / | worker, manager | Dodanie urządzenia |
| PUT | /{id} | worker, manager | Edycja urządzenia |
| PATCH | /{id}/status | manager | Zmiana statusu |
| DELETE | /{id} | manager | Archiwizacja (soft-delete) |
| GET | /{id}/inspections | worker, manager | Historia przeglądów |
| POST | /{id}/inspections | worker, manager | Nowy wpis przeglądu |
| GET | /{id}/failures | worker, manager | Historia awarii |
| POST | /import | admin | Import z pliku Excel (.xlsx) |

### Awarie — `/api/v1/failures`

| Metoda | Endpoint | Kto może | Opis |
|--------|----------|----------|------|
| POST | / | nurse, worker, manager | Zgłoszenie awarii |
| GET | / | wszyscy zalogowani | Lista z filtrowaniem i paginacją |
| GET | /{id} | wszyscy zalogowani | Szczegóły awarii |
| PATCH | /{id}/status | worker, manager | Zmiana statusu |
| PATCH | /{id}/assign | worker, manager | Przypisanie serwisanta |
| PATCH | /{id}/resolve | worker, manager | Zamknięcie z kosztem naprawy |
| GET | /{id}/history | worker, manager | Historia zmian statusu |

### Słowniki — `/api/v1/categories`, `/api/v1/departments`, `/api/v1/service-providers`

| Metoda | Endpoint | Kto może | Opis |
|--------|----------|----------|------|
| GET | / | wszyscy zalogowani | Lista |
| GET | /{id} | wszyscy zalogowani | Szczegóły |
| POST | / | manager, admin | Dodanie |
| PUT | /{id} | manager, admin | Edycja |
| DELETE | /{id} | admin | Usunięcie |

### Raporty — `/api/v1/reports`

| Metoda | Endpoint | Kto może | Opis |
|--------|----------|----------|------|
| POST | /equipment | worker, manager | Raport urządzeń (PDF/Excel) |
| POST | /failures | worker, manager | Raport awarii z kosztami |
| POST | /inspections | worker, manager | Raport przeglądów |
| GET | /export/gdpr/{userId} | admin | Eksport danych GDPR |

### Powiadomienia — `/api/v1/notifications`

| Metoda | Endpoint | Kto może | Opis |
|--------|----------|----------|------|
| GET | / | worker, manager, admin | Lista powiadomień |

## Baza danych

PostgreSQL 16, dostępna na `localhost:5432`:
- **User:** `trisecmed_user`
- **Password:** `dev_password_123`
- **Database:** `trisecmed`

Można podłączyć się pgAdminem, DBeaverem lub psqlem. Migracje są automatyczne przy starcie w trybie dev.

Żeby dodać nową migrację:

```bash
cd backend
dotnet ef migrations add NazwaMigracji \
  --project src/Trisecmed.Infrastructure \
  --startup-project src/Trisecmed.API \
  --output-dir Data/Migrations \
  --configuration Release
```

## Testy

```bash
cd backend
dotnet test --configuration Release
```

## Porty

| Usługa | Port |
|--------|------|
| Frontend (Blazor WASM) | 5002 |
| Backend API (Docker) | 5000 |
| PostgreSQL | 5432 |
| Nginx | 80 |

## Powiadomienia email

System wysyła automatyczne powiadomienia:
- **Zbliżający się przegląd** — codziennie o 7:00, dla urządzeń z przeglądem w ciągu 30 dni
- **Wygasająca gwarancja** — codziennie o 8:00, dla urządzeń z gwarancją w ciągu 14 dni
- **Nowa awaria** — natychmiast po zgłoszeniu, do Worker/Manager z tego samego oddziału

W trybie dev SMTP wskazuje na `localhost:1025` (nie ma serwera mailowego). Powiadomienia zapisują się do bazy, ale emaile nie dochodzą. Aby testować emaile, podłącz [Mailpit](https://github.com/axllent/mailpit):

```bash
docker run -d -p 1025:1025 -p 8025:8025 axllent/mailpit
```

Emaile będą widoczne pod `http://localhost:8025`.

## Dokumentacja

- `docs/Trivaxa_Specyfikacja_Backend.pdf` — pełna specyfikacja techniczna
- `docs/testy/` — scenariusze testowe do ręcznego testowania (curl)

## Stan implementacji

| Faza | Co robi | Status |
|------|---------|--------|
| 0 - Setup | Struktura, Docker, CI/CD, migracje, health check | Gotowe |
| 1 - Tożsamość | Logowanie JWT, role RBAC, audit log | Gotowe |
| 2 - Aparatura | CRUD urządzeń, przeglądy, import Excel | Gotowe |
| 3 - Awarie | Zgłoszenia, statusy, serwisanci, koszty napraw | Gotowe |
| 4 - Powiadomienia | Hangfire, email (przeglądy, gwarancje, awarie) | Gotowe |
| 5 - Raporty | PDF/Excel, eksport GDPR | Gotowe |
| 6 - Słowniki | Kategorie, oddziały, firmy serwisowe | Gotowe |
| 7 - Testy | Jednostkowe, integracyjne, scenariusze curl | Gotowe |
| 8 - Frontend | Blazor WASM + MudBlazor, pełne pokrycie API | Gotowe |

## Autorzy

- mgr inż. Mateusz Bartoszewicz
- mgr inż. Stanisław Rachwał
