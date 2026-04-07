# 08 — Testy słowników (Kategorie, Oddziały, Serwisanci)

## Wymagania wstępne
- Zalogowany jako Manager lub Admin

---

## Test 8.1 — Lista kategorii

```bash
curl -s http://localhost:5000/api/v1/categories \
  -H "Authorization: Bearer $TOKEN"
```
**Oczekiwany wynik:** `200 OK`, lista kategorii z id, name, description

---

## Test 8.2 — Dodanie kategorii

```bash
curl -s -X POST http://localhost:5000/api/v1/categories \
  -H "Authorization: Bearer $MANAGER_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{ "name": "Diagnostyka obrazowa", "description": "RTG, USG, MRI" }'
```
**Oczekiwany wynik:** `201 Created`

---

## Test 8.3 — Lista oddziałów

```bash
curl -s http://localhost:5000/api/v1/departments \
  -H "Authorization: Bearer $TOKEN"
```
**Oczekiwany wynik:** `200 OK`, lista oddziałów

---

## Test 8.4 — Dodanie oddziału

```bash
curl -s -X POST http://localhost:5000/api/v1/departments \
  -H "Authorization: Bearer $MANAGER_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{ "name": "Oddział Chirurgii", "code": "CHIR" }'
```
**Oczekiwany wynik:** `201 Created`

---

## Test 8.5 — Lista serwisantów

```bash
curl -s http://localhost:5000/api/v1/service-providers \
  -H "Authorization: Bearer $TOKEN"
```
**Oczekiwany wynik:** `200 OK`

---

## Test 8.6 — Dodanie serwisanta

```bash
curl -s -X POST http://localhost:5000/api/v1/service-providers \
  -H "Authorization: Bearer $MANAGER_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "MedService Sp. z o.o.",
    "contactPerson": "Jan Kowalski",
    "email": "serwis@medservice.pl",
    "phone": "+48 123 456 789",
    "taxId": "1234567890"
  }'
```
**Oczekiwany wynik:** `201 Created`

---

## Test 8.7 — Aktualizacja serwisanta

```bash
curl -s -X PUT http://localhost:5000/api/v1/service-providers/$SP_ID \
  -H "Authorization: Bearer $MANAGER_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "MedService Sp. z o.o.",
    "contactPerson": "Anna Nowak",
    "email": "nowy@medservice.pl",
    "phone": "+48 987 654 321",
    "taxId": "1234567890"
  }'
```
**Oczekiwany wynik:** `204 No Content`

---

## Test 8.8 — Usunięcie serwisanta (tylko Admin)

```bash
curl -s -X DELETE http://localhost:5000/api/v1/service-providers/$SP_ID \
  -H "Authorization: Bearer $ADMIN_TOKEN"
```
**Oczekiwany wynik:** `204 No Content`

---

## Test 8.9 — Worker nie może usunąć serwisanta (RBAC)

```bash
curl -s -X DELETE http://localhost:5000/api/v1/service-providers/$SP_ID \
  -H "Authorization: Bearer $WORKER_TOKEN"
```
**Oczekiwany wynik:** `403 Forbidden`
