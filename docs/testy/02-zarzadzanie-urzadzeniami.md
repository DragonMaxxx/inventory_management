# Scenariusz 2: Zarządzanie urządzeniami

Ten scenariusz prowadzi przez pełny cykl życia urządzenia — od dodania, przez edycję, przegląd, aż po kasację.

## Przygotowanie

Zaloguj się jako worker (będzie potrzebny w większości testów):

```bash
WORKER=$(curl -s -X POST http://localhost:5000/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"worker@trisecmed.local","password":"Worker123"}' \
  | python -c "import sys,json; print(json.load(sys.stdin)['accessToken'])")

echo "Token: ${WORKER:0:20}..."
```

---

## Test 2.1: Dodanie urządzenia

```bash
curl -s -X POST http://localhost:5000/api/v1/devices \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $WORKER" \
  -d '{
    "name": "Respirator Drager Savina 300",
    "inventoryNumber": "RESP-001",
    "serialNumber": "SN-2024-001234",
    "manufacturer": "Drager",
    "model": "Savina 300",
    "categoryId": "22222222-2222-2222-2222-222222222222",
    "departmentId": "11111111-1111-1111-1111-111111111111",
    "purchaseDate": "2024-03-15",
    "purchasePrice": 85000.00,
    "warrantyExpires": "2027-03-15",
    "nextInspectionDate": "2026-09-15",
    "notes": "Zakupiony z grantu ministerialnego"
  }'
```

**Oczekiwany wynik:** JSON z `id` nowego urządzenia. Zapisz to ID — będzie potrzebne dalej.

```bash
# Zapisz ID do zmiennej (po wykonaniu powyższego)
DEVICE_ID="<wklej_id_z_odpowiedzi>"
```

---

## Test 2.2: Próba dodania duplikatu

```bash
curl -s -w "\nHTTP: %{http_code}\n" -X POST http://localhost:5000/api/v1/devices \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $WORKER" \
  -d '{
    "name": "Inny respirator",
    "inventoryNumber": "RESP-001",
    "manufacturer": "GE",
    "model": "Carescape R860",
    "categoryId": "22222222-2222-2222-2222-222222222222",
    "departmentId": "11111111-1111-1111-1111-111111111111"
  }'
```

**Oczekiwany wynik:** HTTP 409 Conflict z komunikatem o duplikacie numeru inwentarzowego.

---

## Test 2.3: Walidacja — brakujące pola

```bash
curl -s -w "\nHTTP: %{http_code}\n" -X POST http://localhost:5000/api/v1/devices \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $WORKER" \
  -d '{
    "name": "",
    "inventoryNumber": "",
    "manufacturer": "Siemens",
    "model": "Multix",
    "categoryId": "22222222-2222-2222-2222-222222222222",
    "departmentId": "11111111-1111-1111-1111-111111111111"
  }'
```

**Oczekiwany wynik:** HTTP 400 Bad Request z listą błędów walidacji (pusta nazwa, pusty numer).

---

## Test 2.4: Pobranie szczegółów urządzenia

```bash
curl -s "http://localhost:5000/api/v1/devices/$DEVICE_ID" \
  -H "Authorization: Bearer $WORKER"
```

**Oczekiwany wynik:** Pełne dane urządzenia wraz z nazwą oddziału (`Oddział Chirurgii`) i kategorii (`Aparatura diagnostyczna`).

---

## Test 2.5: Edycja urządzenia

```bash
curl -s -w "\nHTTP: %{http_code}\n" -X PUT \
  "http://localhost:5000/api/v1/devices/$DEVICE_ID" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $WORKER" \
  -d "{
    \"id\": \"$DEVICE_ID\",
    \"name\": \"Respirator Drager Savina 300 Select\",
    \"inventoryNumber\": \"RESP-001\",
    \"manufacturer\": \"Drager\",
    \"model\": \"Savina 300 Select\",
    \"categoryId\": \"22222222-2222-2222-2222-222222222222\",
    \"departmentId\": \"11111111-1111-1111-1111-111111111111\",
    \"notes\": \"Model zaktualizowany po upgrade firmware\"
  }"
```

**Oczekiwany wynik:** HTTP 204 No Content. Urządzenie zaktualizowane.

Sprawdź czy dane się zmieniły:

```bash
curl -s "http://localhost:5000/api/v1/devices/$DEVICE_ID" \
  -H "Authorization: Bearer $WORKER" \
  | python -c "import sys,json; d=json.load(sys.stdin); print(f'{d[\"name\"]} | {d[\"model\"]}')"
```

---

## Test 2.6: Dodanie kilku urządzeń (do testów filtrowania)

```bash
for i in 1 2 3 4 5; do
  curl -s -X POST http://localhost:5000/api/v1/devices \
    -H "Content-Type: application/json" \
    -H "Authorization: Bearer $WORKER" \
    -d "{
      \"name\": \"Pompa infuzyjna Braun $i\",
      \"inventoryNumber\": \"PUMP-00$i\",
      \"manufacturer\": \"B.Braun\",
      \"model\": \"Infusomat Space\",
      \"categoryId\": \"22222222-2222-2222-2222-222222222222\",
      \"departmentId\": \"11111111-1111-1111-1111-111111111111\",
      \"purchasePrice\": $((3000 + i * 500))
    }" > /dev/null
done
echo "Dodano 5 pomp infuzyjnych"
```

---

## Test 2.7: Filtrowanie i paginacja

```bash
# Wszystkie urządzenia
echo "=== Wszystkie ==="
curl -s "http://localhost:5000/api/v1/devices" \
  -H "Authorization: Bearer $WORKER" \
  | python -c "import sys,json; d=json.load(sys.stdin); print(f'Razem: {d[\"totalCount\"]}, strona {d[\"page\"]}/{d[\"totalPages\"]}')"

# Wyszukiwanie
echo "=== Szukaj: Braun ==="
curl -s "http://localhost:5000/api/v1/devices?search=Braun" \
  -H "Authorization: Bearer $WORKER" \
  | python -c "import sys,json; d=json.load(sys.stdin); print(f'Znaleziono: {d[\"totalCount\"]}')"

# Sortowanie po nazwie malejąco
echo "=== Sortowanie: nazwa desc ==="
curl -s "http://localhost:5000/api/v1/devices?sortBy=name&sortDir=desc&pageSize=3" \
  -H "Authorization: Bearer $WORKER" \
  | python -c "import sys,json; d=json.load(sys.stdin); [print(f'  {i[\"name\"]}') for i in d['items']]"

# Paginacja — strona 2 po 2 elementy
echo "=== Strona 2, po 2 ==="
curl -s "http://localhost:5000/api/v1/devices?page=2&pageSize=2" \
  -H "Authorization: Bearer $WORKER" \
  | python -c "import sys,json; d=json.load(sys.stdin); print(f'Strona {d[\"page\"]}/{d[\"totalPages\"]}, pokazano {len(d[\"items\"])} z {d[\"totalCount\"]}')"
```

---

## Test 2.8: Urządzenia oddziału

```bash
curl -s "http://localhost:5000/api/v1/devices/department/11111111-1111-1111-1111-111111111111" \
  -H "Authorization: Bearer $WORKER" \
  | python -c "import sys,json; d=json.load(sys.stdin); print(f'{len(d)} urządzeń w Chirurgii')"
```

---

## Test 2.9: Dodanie przeglądu

```bash
curl -s -X POST "http://localhost:5000/api/v1/devices/$DEVICE_ID/inspections" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $WORKER" \
  -d "{
    \"deviceId\": \"$DEVICE_ID\",
    \"inspectionDate\": \"2026-04-05\",
    \"nextInspectionDate\": \"2026-10-05\",
    \"result\": \"Pozytywny — urządzenie sprawne\",
    \"performedBy\": \"Serwis MedTech Sp. z o.o.\",
    \"notes\": \"Wymieniono filtr powietrza\"
  }"
```

**Oczekiwany wynik:** HTTP 201 Created z ID przeglądu.

Sprawdź historię przeglądów:

```bash
curl -s "http://localhost:5000/api/v1/devices/$DEVICE_ID/inspections" \
  -H "Authorization: Bearer $WORKER"
```

---

## Test 2.10: Zmiana statusu (wymaga managera)

```bash
MANAGER=$(curl -s -X POST http://localhost:5000/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"manager@trisecmed.local","password":"Manager123"}' \
  | python -c "import sys,json; print(json.load(sys.stdin)['accessToken'])")

# Zmiana na InRepair
curl -s -w "\nHTTP: %{http_code}\n" -X PATCH \
  "http://localhost:5000/api/v1/devices/$DEVICE_ID/status" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $MANAGER" \
  -d '{"status": "InRepair"}'
```

**Oczekiwany wynik:** HTTP 204. Status zmieniony.

Sprawdź:

```bash
curl -s "http://localhost:5000/api/v1/devices/$DEVICE_ID" \
  -H "Authorization: Bearer $WORKER" \
  | python -c "import sys,json; d=json.load(sys.stdin); print(f'Status: {d[\"status\"]}')"
```

---

## Test 2.11: Kasacja urządzenia (soft-delete)

```bash
curl -s -w "\nHTTP: %{http_code}\n" -X DELETE \
  "http://localhost:5000/api/v1/devices/$DEVICE_ID" \
  -H "Authorization: Bearer $MANAGER"
```

**Oczekiwany wynik:** HTTP 204. Urządzenie nadal istnieje w bazie, ale ze statusem `Archived`.

```bash
curl -s "http://localhost:5000/api/v1/devices/$DEVICE_ID" \
  -H "Authorization: Bearer $WORKER" \
  | python -c "import sys,json; d=json.load(sys.stdin); print(f'Status: {d[\"status\"]}')"
# Powinno pokazać: Status: Archived
```

**Urządzenie nie znikło z listy** — możesz je odfiltrować:

```bash
# Tylko aktywne
curl -s "http://localhost:5000/api/v1/devices?status=Active" \
  -H "Authorization: Bearer $WORKER" \
  | python -c "import sys,json; d=json.load(sys.stdin); print(f'Aktywnych: {d[\"totalCount\"]}')"

# Tylko zarchiwizowane
curl -s "http://localhost:5000/api/v1/devices?status=Archived" \
  -H "Authorization: Bearer $WORKER" \
  | python -c "import sys,json; d=json.load(sys.stdin); print(f'Zarchiwizowanych: {d[\"totalCount\"]}')"
```
