# Scenariusz 5: Macierz uprawnień RBAC

Ten scenariusz systematycznie sprawdza, czy każda rola ma dostęp tylko do operacji, do których powinna mieć.

## Przygotowanie

Zaloguj się na wszystkie trzy konta:

```bash
ADMIN=$(curl -s -X POST http://localhost:5000/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@trisecmed.local","password":"Admin123"}' \
  | python -c "import sys,json; print(json.load(sys.stdin)['accessToken'])")

WORKER=$(curl -s -X POST http://localhost:5000/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"worker@trisecmed.local","password":"Worker123"}' \
  | python -c "import sys,json; print(json.load(sys.stdin)['accessToken'])")

MANAGER=$(curl -s -X POST http://localhost:5000/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"manager@trisecmed.local","password":"Manager123"}' \
  | python -c "import sys,json; print(json.load(sys.stdin)['accessToken'])")

echo "Zalogowano 3 użytkowników"
```

Dodaj urządzenie do testów (jako worker):

```bash
DEVICE_ID=$(curl -s -X POST http://localhost:5000/api/v1/devices \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $WORKER" \
  -d '{
    "name": "Test RBAC",
    "inventoryNumber": "RBAC-TEST-001",
    "manufacturer": "TestCo",
    "model": "Model X",
    "categoryId": "22222222-2222-2222-2222-222222222222",
    "departmentId": "11111111-1111-1111-1111-111111111111"
  }' | python -c "import sys,json; print(json.load(sys.stdin)['id'])")

echo "Device ID: $DEVICE_ID"
```

---

## Tabela testów

Poniższe komendy testują kluczowe operacje dla każdej roli. Przy każdej sprawdź kod HTTP.

### GET /devices — lista urządzeń

```bash
echo "Admin:   $(curl -s -o /dev/null -w '%{http_code}' http://localhost:5000/api/v1/devices -H "Authorization: Bearer $ADMIN")"
echo "Worker:  $(curl -s -o /dev/null -w '%{http_code}' http://localhost:5000/api/v1/devices -H "Authorization: Bearer $WORKER")"
echo "Manager: $(curl -s -o /dev/null -w '%{http_code}' http://localhost:5000/api/v1/devices -H "Authorization: Bearer $MANAGER")"
```

**Oczekiwane:** Admin 200, Worker 200, Manager 200.

---

### POST /devices — dodanie urządzenia

```bash
BODY='{"name":"Test","inventoryNumber":"RBAC-A","manufacturer":"X","model":"Y","categoryId":"22222222-2222-2222-2222-222222222222","departmentId":"11111111-1111-1111-1111-111111111111"}'

echo "Admin:   $(curl -s -o /dev/null -w '%{http_code}' -X POST http://localhost:5000/api/v1/devices -H "Content-Type: application/json" -H "Authorization: Bearer $ADMIN" -d "$BODY")"

BODY='{"name":"Test","inventoryNumber":"RBAC-B","manufacturer":"X","model":"Y","categoryId":"22222222-2222-2222-2222-222222222222","departmentId":"11111111-1111-1111-1111-111111111111"}'
echo "Worker:  $(curl -s -o /dev/null -w '%{http_code}' -X POST http://localhost:5000/api/v1/devices -H "Content-Type: application/json" -H "Authorization: Bearer $WORKER" -d "$BODY")"

BODY='{"name":"Test","inventoryNumber":"RBAC-C","manufacturer":"X","model":"Y","categoryId":"22222222-2222-2222-2222-222222222222","departmentId":"11111111-1111-1111-1111-111111111111"}'
echo "Manager: $(curl -s -o /dev/null -w '%{http_code}' -X POST http://localhost:5000/api/v1/devices -H "Content-Type: application/json" -H "Authorization: Bearer $MANAGER" -d "$BODY")"
```

**Oczekiwane:** Admin 403, Worker 201, Manager 201.

---

### PATCH /devices/{id}/status — zmiana statusu

```bash
echo "Admin:   $(curl -s -o /dev/null -w '%{http_code}' -X PATCH "http://localhost:5000/api/v1/devices/$DEVICE_ID/status" -H "Content-Type: application/json" -H "Authorization: Bearer $ADMIN" -d '{"status":"InRepair"}')"
echo "Worker:  $(curl -s -o /dev/null -w '%{http_code}' -X PATCH "http://localhost:5000/api/v1/devices/$DEVICE_ID/status" -H "Content-Type: application/json" -H "Authorization: Bearer $WORKER" -d '{"status":"InRepair"}')"
echo "Manager: $(curl -s -o /dev/null -w '%{http_code}' -X PATCH "http://localhost:5000/api/v1/devices/$DEVICE_ID/status" -H "Content-Type: application/json" -H "Authorization: Bearer $MANAGER" -d '{"status":"InRepair"}')"
```

**Oczekiwane:** Admin 403, Worker 403, Manager 204.

---

### DELETE /devices/{id} — kasacja

```bash
echo "Admin:   $(curl -s -o /dev/null -w '%{http_code}' -X DELETE "http://localhost:5000/api/v1/devices/$DEVICE_ID" -H "Authorization: Bearer $ADMIN")"
echo "Worker:  $(curl -s -o /dev/null -w '%{http_code}' -X DELETE "http://localhost:5000/api/v1/devices/$DEVICE_ID" -H "Authorization: Bearer $WORKER")"
echo "Manager: $(curl -s -o /dev/null -w '%{http_code}' -X DELETE "http://localhost:5000/api/v1/devices/$DEVICE_ID" -H "Authorization: Bearer $MANAGER")"
```

**Oczekiwane:** Admin 403, Worker 403, Manager 204.

---

### GET /users — lista użytkowników

```bash
echo "Admin:   $(curl -s -o /dev/null -w '%{http_code}' http://localhost:5000/api/v1/users -H "Authorization: Bearer $ADMIN")"
echo "Worker:  $(curl -s -o /dev/null -w '%{http_code}' http://localhost:5000/api/v1/users -H "Authorization: Bearer $WORKER")"
echo "Manager: $(curl -s -o /dev/null -w '%{http_code}' http://localhost:5000/api/v1/users -H "Authorization: Bearer $MANAGER")"
```

**Oczekiwane:** Admin 200, Worker 403, Manager 403.

---

## Podsumowanie oczekiwanych wyników

| Operacja | Admin | Worker | Manager |
|----------|-------|--------|---------|
| GET /devices | 200 | 200 | 200 |
| POST /devices | 403 | 201 | 201 |
| PUT /devices/{id} | 403 | 204 | 204 |
| PATCH /devices/{id}/status | 403 | 403 | 204 |
| DELETE /devices/{id} | 403 | 403 | 204 |
| POST /devices/import | 200 | 403 | 403 |
| GET /users | 200 | 403 | 403 |
| POST /users | 201 | 403 | 403 |
