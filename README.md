# Trisecmed - System ewidencji aparatury medycznej

System ewidencji i zarządzania aparaturą medyczną dla szpitali.
Backend API zbudowany w architekturze Clean Architecture.

**Stack:** C# / .NET 10 / ASP.NET Core / EF Core / PostgreSQL 16 / MediatR / FluentValidation / Hangfire / Serilog / Docker

## Szybki start

### Wymagania
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (uruchomiony)
- [.NET 10 SDK](https://dotnet.microsoft.com/) (opcjonalnie, do dev lokalnego)
- Git

### 1. Uruchomienie (Docker - rekomendowane)

```bash
# Sklonuj repo i wejdź do katalogu
cd inventory_management

# Skopiuj .env (edytuj hasła jeśli potrzeba)
cp .env.example .env

# Uruchom wszystko
docker compose up --build -d

# Sprawdź status
docker compose ps
```

API będzie dostępne na **http://localhost:5000**.

### 2. Weryfikacja

```bash
# Health check
curl http://localhost:5000/health
# Powinno zwrócić: Healthy

# Dokumentacja API (otwórz w przeglądarce)
# http://localhost:5000/scalar/v1
```

### 3. Zatrzymanie

```bash
docker compose down          # zatrzymaj kontenery
docker compose down -v       # zatrzymaj + usuń dane (reset bazy)
```

## Konto administratora (dev)

Przy pierwszym uruchomieniu tworzony jest admin:

| Pole | Wartość |
|------|---------|
| Email | `admin@trisecmed.local` |
| Hasło | `Admin123` |
| Rola | `Administrator` |

## API - endpointy

### Autentykacja (`/api/v1/auth`)

| Metoda | Endpoint | Auth | Opis |
|--------|----------|------|------|
| POST | `/auth/login` | - | Logowanie, zwraca JWT access + refresh token |
| POST | `/auth/refresh` | - | Odświeżenie access tokenu |
| POST | `/auth/logout` | JWT | Wylogowanie (unieważnia refresh token) |
| POST | `/auth/activate` | - | Aktywacja konta tokenem |

**Przykład logowania:**
```bash
curl -X POST http://localhost:5000/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@trisecmed.local","password":"Admin123"}'
```

**Użycie tokenu:**
```bash
TOKEN="<access_token_z_logowania>"
curl http://localhost:5000/api/v1/users \
  -H "Authorization: Bearer $TOKEN"
```

### Użytkownicy (`/api/v1/users`) - tylko admin

| Metoda | Endpoint | Opis |
|--------|----------|------|
| GET | `/users` | Lista użytkowników |
| POST | `/users` | Utworzenie konta (z tokenem aktywacyjnym) |

**Przykład tworzenia użytkownika:**
```bash
curl -X POST http://localhost:5000/api/v1/users \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{"email":"jan@szpital.pl","firstName":"Jan","lastName":"Kowalski","role":"EquipmentWorker","departmentId":"11111111-1111-1111-1111-111111111111"}'
```

### Urządzenia (`/api/v1/devices`)

| Metoda | Endpoint | Opis |
|--------|----------|------|
| GET | `/devices` | Lista urządzeń |
| POST | `/devices` | Dodanie urządzenia |

**Przykład dodania urządzenia:**
```bash
curl -X POST http://localhost:5000/api/v1/devices \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Aparat RTG Siemens",
    "inventoryNumber": "RTG-001",
    "manufacturer": "Siemens",
    "model": "Multix Impact",
    "categoryId": "22222222-2222-2222-2222-222222222222",
    "departmentId": "11111111-1111-1111-1111-111111111111"
  }'
```

## Seed data (dane testowe)

Przy starcie w trybie Development automatycznie tworzone są:

**Oddziały:**
| ID | Nazwa | Kod |
|----|-------|-----|
| `11111111-...` | Oddział Chirurgii | CHIR |
| (auto) | Oddział Kardiologii | KARD |
| (auto) | Oddział Intensywnej Terapii | OIT |

**Kategorie urządzeń:**
| ID | Nazwa |
|----|-------|
| `22222222-...` | Aparatura diagnostyczna |
| (auto) | Aparatura terapeutyczna |
| (auto) | Sprzęt laboratoryjny |
| (auto) | Wyposażenie pomocnicze |

## Architektura

```
backend/
├── Trisecmed.slnx                    # Solution file
├── Dockerfile                         # Multi-stage Docker build
└── src/
    ├── Trisecmed.Domain/              # Encje, enumy, interfejsy (0 zależności)
    ├── Trisecmed.Application/         # CQRS (MediatR), walidacja, DTOs
    ├── Trisecmed.Infrastructure/      # EF Core, repozytoria, JWT, BCrypt, audit
    └── Trisecmed.API/                 # Kontrolery, middleware, Program.cs
```

**Wzorce:** Modular Monolith, CQRS (MediatR), Repository Pattern, Result Pattern, Unit of Work

## Rozwój lokalny (bez Docker)

Jeśli chcesz uruchamiać API lokalnie (szybszy dev cycle):

```bash
# 1. Uruchom tylko bazę w Docker
docker compose up db -d

# 2. Uruchom API lokalnie
cd backend
dotnet run --project src/Trisecmed.API --configuration Release --urls "http://localhost:5008"
```

> **Uwaga WDAC:** Na tym komputerze Windows WDAC blokuje DLL-ki w trybie Debug.
> Zawsze używaj `--configuration Release` do uruchamiania i migracji EF Core.

### Migracje EF Core

```bash
cd backend

# Nowa migracja
dotnet ef migrations add NazwaMigracji \
  --project src/Trisecmed.Infrastructure \
  --startup-project src/Trisecmed.API \
  --output-dir Data/Migrations \
  --configuration Release

# Aplikacja migracji
dotnet ef database update \
  --project src/Trisecmed.Infrastructure \
  --startup-project src/Trisecmed.API \
  --configuration Release
```

## Baza danych

**Połączenie (dev):**
| Parametr | Wartość |
|----------|---------|
| Host | `localhost` |
| Port | `5432` |
| Database | `trisecmed` |
| User | `trisecmed_user` |
| Password | `dev_password_123` |

Możesz podłączyć się przez pgAdmin, DBeaver lub psql.

## Role użytkowników (RBAC)

| Rola | Opis |
|------|------|
| `Nurse` | Pielęgniarka — podgląd urządzeń oddziału, zgłaszanie awarii |
| `EquipmentWorker` | Pracownik DAM — CRUD urządzeń, obsługa awarii, raporty |
| `EquipmentManager` | Kierownik DAM — kasacja urządzeń, konfiguracja słowników |
| `Administrator` | Admin — zarządzanie użytkownikami, migracje |

## Porty

| Usługa | Port | Opis |
|--------|------|------|
| API (Docker) | 5000 | Główne API przez Docker |
| API (lokalne) | 5008 | Dev z `dotnet run` |
| PostgreSQL | 5432 | Baza danych |
| Nginx | 80 | Reverse proxy (Docker) |

## Harmonogram implementacji

| Faza | Zakres | Status |
|------|--------|--------|
| 0 - Setup | Struktura, Docker, migracje, health check | Gotowe |
| 1 - Tożsamość | JWT auth, RBAC, audit log | Gotowe |
| 2 - Aparatura | Pełny CRUD urządzeń, przeglądy, import Excel | Do zrobienia |
| 3 - Awarie | Zgłoszenia, statusy, historia, koszty | Do zrobienia |
| 4 - Powiadomienia | Hangfire jobs, email, szablony | Do zrobienia |
| 5 - Raporty | PDF (QuestPDF), XLSX (ClosedXML), RODO export | Do zrobienia |
| 6 - Serwisanci | Firmy serwisowe, słowniki, panel admina | Do zrobienia |
| 7 - Testy | Penetracyjne, wydajnościowe, audyt | Do zrobienia |

## Autorzy

- mgr inż. Mateusz Bartoszewicz
- mgr inż. Stanisław Rachwał
