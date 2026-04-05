# Scenariusze testowe — Trisecmed

Zbiór scenariuszy do ręcznego testowania systemu. Każdy scenariusz zawiera gotowe komendy curl, które można kopiować i wklejać do terminala.

## Wymagania

- Docker Desktop uruchomiony
- System uruchomiony (`docker compose up -d`)
- Dostęp do terminala z curl i python (Git Bash na Windowsie działa)

## Reset systemu

Jeśli chcesz zacząć od czystej bazy:

```bash
docker compose down -v
docker compose up --build -d
```

## Lista scenariuszy

1. **[Logowanie i autoryzacja](01-logowanie-i-autoryzacja.md)** — logowanie, błędne dane, tokeny JWT, wylogowanie
2. **[Zarządzanie urządzeniami](02-zarzadzanie-urzadzeniami.md)** — dodawanie, edycja, przeglądy, filtrowanie, paginacja, kasacja
3. **[Zarządzanie użytkownikami](03-zarzadzanie-uzytkownikami.md)** — tworzenie kont, lista użytkowników, kontrola ról
4. **[Import z Excela](04-import-excel.md)** — import urządzeń z pliku .xlsx, duplikaty, błędy
5. **[Macierz uprawnień RBAC](05-rbac-macierz-uprawnien.md)** — systematyczny test uprawnień dla każdej roli

## Kolejność

Scenariusze najlepiej przechodzić w kolejności 1 → 2 → 3 → 4 → 5, bo niektóre (np. 2) tworzą dane potrzebne w późniejszych testach. Scenariusz 5 (RBAC) można robić niezależnie od reszty.

## Stałe ID do testów

W seedzie są stałe GUID-y — przydatne przy wklejaniu do curl:

| Co | ID |
|----|----|
| Oddział Chirurgii | `11111111-1111-1111-1111-111111111111` |
| Aparatura diagnostyczna | `22222222-2222-2222-2222-222222222222` |
| Admin | `33333333-3333-3333-3333-333333333333` |
| Worker | `44444444-4444-4444-4444-444444444444` |
| Manager | `55555555-5555-5555-5555-555555555555` |
