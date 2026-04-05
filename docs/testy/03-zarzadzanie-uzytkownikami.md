# Scenariusz 3: Zarządzanie użytkownikami

Tylko administrator może tworzyć konta i przeglądać listę użytkowników.

## Przygotowanie

```bash
ADMIN=$(curl -s -X POST http://localhost:5000/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@trisecmed.local","password":"Admin123"}' \
  | python -c "import sys,json; print(json.load(sys.stdin)['accessToken'])")
```

---

## Test 3.1: Lista użytkowników

```bash
curl -s http://localhost:5000/api/v1/users \
  -H "Authorization: Bearer $ADMIN"
```

**Oczekiwany wynik:** JSON z listą 3 użytkowników (admin, worker, manager + ewentualni dodani wcześniej).

---

## Test 3.2: Worker nie widzi listy użytkowników

```bash
WORKER=$(curl -s -X POST http://localhost:5000/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"worker@trisecmed.local","password":"Worker123"}' \
  | python -c "import sys,json; print(json.load(sys.stdin)['accessToken'])")

curl -s -w "\nHTTP: %{http_code}\n" http://localhost:5000/api/v1/users \
  -H "Authorization: Bearer $WORKER"
```

**Oczekiwany wynik:** HTTP 403 Forbidden.

---

## Test 3.3: Utworzenie nowego konta

```bash
curl -s -X POST http://localhost:5000/api/v1/users \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $ADMIN" \
  -d '{
    "email": "anna.pielegniarki@szpital.pl",
    "firstName": "Anna",
    "lastName": "Wisniewska",
    "role": "Nurse",
    "departmentId": "11111111-1111-1111-1111-111111111111"
  }'
```

**Oczekiwany wynik:** HTTP 201 Created. Nowe konto zostało utworzone z tymczasowym hasłem i tokenem aktywacyjnym. W rzeczywistym systemie użytkownik dostałby email z linkiem do aktywacji.

---

## Test 3.4: Sprawdzenie czy nowy użytkownik jest na liście

```bash
curl -s http://localhost:5000/api/v1/users \
  -H "Authorization: Bearer $ADMIN" \
  | python -c "
import sys, json
users = json.load(sys.stdin)
for u in users:
    print(f'{u[\"email\"]:40} {u[\"role\"]:20} active={u[\"isActive\"]}')
"
```

**Oczekiwany wynik:** Nowy użytkownik powinien być na liście z rolą `Nurse`.
