# 07 — Testy modułu raportów

## Wymagania wstępne
- Zalogowany jako Worker lub Manager
- Dane urządzeń/awarii/przeglądów w bazie

---

## Test 7.1 — Raport aparatury PDF

```bash
curl -s -X POST http://localhost:5000/api/v1/reports/equipment \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{ "format": "pdf" }' \
  --output raport-aparatura.pdf
```
**Oczekiwany wynik:** plik PDF z tabelą urządzeń

---

## Test 7.2 — Raport aparatury XLSX

```bash
curl -s -X POST http://localhost:5000/api/v1/reports/equipment \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{ "format": "xlsx", "status": "Active" }' \
  --output raport-aparatura.xlsx
```
**Oczekiwany wynik:** plik Excel z kolumnami: Nazwa, Nr inwentarzowy, Producent, Status...

---

## Test 7.3 — Raport awarii z zakresem dat

```bash
curl -s -X POST http://localhost:5000/api/v1/reports/failures \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{ "dateFrom": "2026-01-01", "dateTo": "2026-12-31", "format": "pdf" }' \
  --output raport-awarie.pdf
```
**Oczekiwany wynik:** PDF z tabelą awarii i podsumowaniem kosztów

---

## Test 7.4 — Raport przeglądów XLSX

```bash
curl -s -X POST http://localhost:5000/api/v1/reports/inspections \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{ "format": "xlsx" }' \
  --output raport-przeglady.xlsx
```
**Oczekiwany wynik:** Excel z listą przeglądów

---

## Test 7.5 — Eksport RODO (tylko Admin)

```bash
curl -s http://localhost:5000/api/v1/reports/export/gdpr/$USER_ID \
  -H "Authorization: Bearer $ADMIN_TOKEN" \
  --output gdpr-export.xlsx
```
**Oczekiwany wynik:** Excel z zakładkami: Dane użytkownika, Log audytu, Zgłoszone awarie

---

## Test 7.6 — Worker nie może eksportować RODO (RBAC)

```bash
curl -s http://localhost:5000/api/v1/reports/export/gdpr/$USER_ID \
  -H "Authorization: Bearer $WORKER_TOKEN"
```
**Oczekiwany wynik:** `403 Forbidden`
