# 06 — Testy modułu awarii

## Wymagania wstępne
- Zalogowany jako Worker lub Nurse (token JWT)
- Istniejące urządzenie w bazie
- Zmienne: `$TOKEN`, `$DEVICE_ID`, `$DEPT_ID`

---

## Test 6.1 — Zgłoszenie awarii (Nurse)

```bash
curl -s -X POST http://localhost:5000/api/v1/failures \
  -H "Authorization: Bearer $NURSE_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "deviceId": "'$DEVICE_ID'",
    "reportedByUserId": "'$NURSE_USER_ID'",
    "departmentId": "'$DEPT_ID'",
    "description": "Aparat nie włącza się po naciśnięciu przycisku power",
    "priority": "High"
  }'
```
**Oczekiwany wynik:** `201 Created`, zwraca `{ "id": "..." }`

---

## Test 6.2 — Lista awarii z filtrowaniem

```bash
curl -s "http://localhost:5000/api/v1/failures?status=Open&priority=High&page=1&pageSize=10" \
  -H "Authorization: Bearer $TOKEN"
```
**Oczekiwany wynik:** `200 OK`, paginowany wynik z filtrami

---

## Test 6.3 — Szczegóły awarii

```bash
curl -s http://localhost:5000/api/v1/failures/$FAILURE_ID \
  -H "Authorization: Bearer $TOKEN"
```
**Oczekiwany wynik:** `200 OK`, pełne dane awarii z nazwą urządzenia i oddziału

---

## Test 6.4 — Zmiana statusu (Worker)

```bash
curl -s -X PATCH http://localhost:5000/api/v1/failures/$FAILURE_ID/status \
  -H "Authorization: Bearer $WORKER_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{ "status": "InProgress", "notes": "Rozpoczęto diagnostykę" }'
```
**Oczekiwany wynik:** `204 No Content`

---

## Test 6.5 — Przypisanie serwisanta

```bash
curl -s -X PATCH http://localhost:5000/api/v1/failures/$FAILURE_ID/assign \
  -H "Authorization: Bearer $WORKER_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{ "serviceProviderId": "'$SP_ID'" }'
```
**Oczekiwany wynik:** `204 No Content`

---

## Test 6.6 — Zamknięcie awarii z kosztem

```bash
curl -s -X PATCH http://localhost:5000/api/v1/failures/$FAILURE_ID/resolve \
  -H "Authorization: Bearer $WORKER_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "failureId": "'$FAILURE_ID'",
    "resolvedByUserId": "'$WORKER_USER_ID'",
    "repairCost": 2500.00,
    "repairDescription": "Wymiana zasilacza i płyty głównej"
  }'
```
**Oczekiwany wynik:** `204 No Content`

---

## Test 6.7 — Historia zmian statusu

```bash
curl -s http://localhost:5000/api/v1/failures/$FAILURE_ID/history \
  -H "Authorization: Bearer $TOKEN"
```
**Oczekiwany wynik:** `200 OK`, lista wpisów z oldStatus, newStatus, changedByUserName, notes

---

## Test 6.8 — Nurse nie może zmieniać statusu (RBAC)

```bash
curl -s -X PATCH http://localhost:5000/api/v1/failures/$FAILURE_ID/status \
  -H "Authorization: Bearer $NURSE_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{ "status": "Resolved" }'
```
**Oczekiwany wynik:** `403 Forbidden`
