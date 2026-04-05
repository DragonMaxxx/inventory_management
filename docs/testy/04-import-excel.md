# Scenariusz 4: Import urządzeń z Excela

System pozwala zaimportować urządzenia z pliku .xlsx. Import robi tylko administrator.

## Przygotowanie

Zaloguj się jako admin:

```bash
ADMIN=$(curl -s -X POST http://localhost:5000/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@trisecmed.local","password":"Admin123"}' \
  | python -c "import sys,json; print(json.load(sys.stdin)['accessToken'])")
```

## Przygotowanie pliku Excel

Utwórz plik `urzadzenia.xlsx` w Excelu z nagłówkami w pierwszym wierszu:

| Name | InventoryNumber | SerialNumber | Manufacturer | Model | Notes |
|------|-----------------|--------------|--------------|-------|-------|
| Defibrylator Philips | DEF-001 | PH-2024-001 | Philips | HeartStart XL | Oddział kardiologii |
| Monitor pacjenta | MON-001 | GE-2024-555 | GE Healthcare | B450 | |
| Pompa infuzyjna | PUMP-100 | BB-2024-100 | B.Braun | Infusomat Space | |
| Aparat EKG | EKG-001 | | Nihon Kohden | ECG-2550 | Przenośny |
| Respirator | | | Drager | Evita V300 | Brak nr inwentarzowego |

Kolumna "Name" może się też nazywać "Nazwa", "InventoryNumber" może być "Numer inwentarzowy" itd.

---

## Test 4.1: Import prawidłowego pliku

```bash
curl -s -X POST http://localhost:5000/api/v1/devices/import \
  -H "Authorization: Bearer $ADMIN" \
  -F "file=@urzadzenia.xlsx"
```

**Oczekiwany wynik:** JSON z raportem:
- `imported` — ile urządzeń dodano (4 — bo ostatni wiersz nie ma numeru inwentarzowego)
- `duplicates` — ile pominięto jako duplikaty (0 przy pierwszym imporcie)
- `errors` — lista błędów z numerem wiersza i opisem (wiersz 6: brak numeru inwentarzowego)

---

## Test 4.2: Ponowny import tego samego pliku

```bash
curl -s -X POST http://localhost:5000/api/v1/devices/import \
  -H "Authorization: Bearer $ADMIN" \
  -F "file=@urzadzenia.xlsx"
```

**Oczekiwany wynik:** `imported: 0`, `duplicates: 4` — system rozpoznał że te numery inwentarzowe już istnieją.

---

## Test 4.3: Worker nie może importować

```bash
WORKER=$(curl -s -X POST http://localhost:5000/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"worker@trisecmed.local","password":"Worker123"}' \
  | python -c "import sys,json; print(json.load(sys.stdin)['accessToken'])")

curl -s -w "\nHTTP: %{http_code}\n" -X POST http://localhost:5000/api/v1/devices/import \
  -H "Authorization: Bearer $WORKER" \
  -F "file=@urzadzenia.xlsx"
```

**Oczekiwany wynik:** HTTP 403 Forbidden.

---

## Test 4.4: Nieprawidłowy format pliku

```bash
echo "to nie jest excel" > test.txt
curl -s -w "\nHTTP: %{http_code}\n" -X POST http://localhost:5000/api/v1/devices/import \
  -H "Authorization: Bearer $ADMIN" \
  -F "file=@test.txt"
rm test.txt
```

**Oczekiwany wynik:** HTTP 400 Bad Request — akceptowany format to tylko .xlsx.

---

## Test 4.5: Sprawdzenie zaimportowanych urządzeń

```bash
curl -s "http://localhost:5000/api/v1/devices?search=Philips" \
  -H "Authorization: Bearer $WORKER" \
  | python -c "import sys,json; d=json.load(sys.stdin); [print(f'{i[\"name\"]} | {i[\"inventoryNumber\"]}') for i in d['items']]"
```

**Oczekiwany wynik:** Zaimportowany defibrylator Philips powinien być na liście.

## Uwagi

- Importowane urządzenia trafiają domyślnie do oddziału Chirurgii (CHIR) i kategorii "Aparatura diagnostyczna" — docelowo admin będzie mógł to skonfigurować
- Puste wiersze w Excelu są pomijane
- System próbuje mapować kolumny po nazwie — obsługuje zarówno angielskie jak i polskie nazwy kolumn
