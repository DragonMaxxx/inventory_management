# Scenariusz 1: Logowanie i autoryzacja

Celem tego scenariusza jest sprawdzenie czy system poprawnie obsługuje logowanie, tokeny JWT i kontrolę dostępu.

## Przygotowanie

Upewnij się, że Docker jest uruchomiony i system działa:

```bash
docker compose up -d
curl http://localhost:5000/health
# Oczekiwany wynik: Healthy
```

---

## Test 1.1: Poprawne logowanie (admin)

```bash
curl -s -X POST http://localhost:5000/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@trisecmed.local","password":"Admin123"}'
```

**Oczekiwany wynik:** JSON z polami `accessToken`, `refreshToken` i `accessTokenExpires`. Token wygasa po 15 minutach.

---

## Test 1.2: Błędne hasło

```bash
curl -s -w "\nHTTP: %{http_code}\n" -X POST http://localhost:5000/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@trisecmed.local","password":"ZleHaslo"}'
```

**Oczekiwany wynik:** HTTP 401 Unauthorized.

---

## Test 1.3: Nieistniejący email

```bash
curl -s -w "\nHTTP: %{http_code}\n" -X POST http://localhost:5000/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"ktos@nieistnieje.pl","password":"test"}'
```

**Oczekiwany wynik:** HTTP 401 Unauthorized.

---

## Test 1.4: Zapytanie bez tokenu

```bash
curl -s -w "\nHTTP: %{http_code}\n" http://localhost:5000/api/v1/devices
```

**Oczekiwany wynik:** HTTP 401 Unauthorized. Nie da się pobrać listy urządzeń bez zalogowania.

---

## Test 1.5: Zapytanie z tokenem

Najpierw zaloguj się i zapisz token:

```bash
TOKEN=$(curl -s -X POST http://localhost:5000/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"worker@trisecmed.local","password":"Worker123"}' \
  | python -c "import sys,json; print(json.load(sys.stdin)['accessToken'])")
```

Teraz użyj tokenu:

```bash
curl -s http://localhost:5000/api/v1/devices \
  -H "Authorization: Bearer $TOKEN"
```

**Oczekiwany wynik:** HTTP 200, JSON z listą urządzeń (na początku pusta).

---

## Test 1.6: Kontrola ról — worker nie może kasować

Zaloguj się jako worker i spróbuj usunąć urządzenie (dowolne ID):

```bash
TOKEN=$(curl -s -X POST http://localhost:5000/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"worker@trisecmed.local","password":"Worker123"}' \
  | python -c "import sys,json; print(json.load(sys.stdin)['accessToken'])")

curl -s -w "\nHTTP: %{http_code}\n" -X DELETE \
  http://localhost:5000/api/v1/devices/11111111-1111-1111-1111-111111111111 \
  -H "Authorization: Bearer $TOKEN"
```

**Oczekiwany wynik:** HTTP 403 Forbidden. Worker nie ma uprawnień do kasacji.

---

## Test 1.7: Odświeżenie tokenu

```bash
# Zaloguj się
RESPONSE=$(curl -s -X POST http://localhost:5000/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"worker@trisecmed.local","password":"Worker123"}')

REFRESH=$(echo "$RESPONSE" | python -c "import sys,json; print(json.load(sys.stdin)['refreshToken'])")

# Odśwież token
curl -s -X POST http://localhost:5000/api/v1/auth/refresh \
  -H "Content-Type: application/json" \
  -d "{\"refreshToken\":\"$REFRESH\"}"
```

**Oczekiwany wynik:** Nowy `accessToken` i `refreshToken`.

---

## Test 1.8: Wylogowanie

```bash
TOKEN=$(curl -s -X POST http://localhost:5000/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"worker@trisecmed.local","password":"Worker123"}' \
  | python -c "import sys,json; print(json.load(sys.stdin)['accessToken'])")

curl -s -w "\nHTTP: %{http_code}\n" -X POST http://localhost:5000/api/v1/auth/logout \
  -H "Authorization: Bearer $TOKEN"
```

**Oczekiwany wynik:** HTTP 200. Refresh token został unieważniony w bazie.
