# API-документація (B-11)

**Інтерактивна документація:** запустіть API (`dotnet run --project src/BloodLink.Api`) і відкрийте <http://localhost:5124/swagger>. Там можна викликати кожен ендпоінт: спочатку `POST /api/auth/login`, потім кнопка **Authorize** і токен.

Машиночитна специфікація OpenAPI 3: [`openapi.json`](openapi.json) (вивантажена з `/swagger/v1/swagger.json`).

## Ендпоінти

| Метод | Шлях | Доступ | Опис | Коди відповіді |
| --- | --- | --- | --- | --- |
| POST | `/api/auth/register` | усі | Реєстрація донора, повертає JWT | 201, 400, 409 |
| POST | `/api/auth/login` | усі | Вхід, повертає JWT | 200, 400, 401 |
| GET | `/api/profile` | JWT | Свій профіль | 200, 401 |
| PUT | `/api/profile` | JWT | Оновити профіль | 200, 400, 401 |
| GET | `/api/eligibility?type=WholeBlood` | JWT | Коли можна здавати наступного разу | 200, 401 |
| GET | `/api/centers?lat=&lng=&limit=` | усі | Центри крові від найближчого | 200, 400 |
| GET | `/api/donations` | JWT | Історія донацій, від нових | 200, 401 |
| POST | `/api/donations` | JWT | Додати донацію | 201, 400, 401 |
| DELETE | `/api/donations/{id}` | JWT | Видалити свій запис | 204, 401, 404 |

Переліки передаються рядками: `sex` — `Male`/`Female`; `bloodGroup` — `O`/`A`/`B`/`AB`; `rhFactor` — `Positive`/`Negative`; `type` — `WholeBlood`/`Plasma`/`Platelets`. Дати — `yyyy-MM-dd`.

## Приклади

Реєстрація:

```http
POST /api/auth/register
Content-Type: application/json

{ "email": "olena@example.com", "password": "Passw0rd!", "fullName": "Олена Коваль" }
```

```json
201 Created
{ "token": "eyJhbGciOi...", "expiresAt": "2026-11-16T12:00:00Z", "donorId": 1, "fullName": "Олена Коваль" }
```

Коли можна здавати:

```http
GET /api/eligibility?type=WholeBlood
Authorization: Bearer eyJhbGciOi...
```

```json
200 OK
{ "type": "WholeBlood", "canDonateNow": false, "nextDate": "2027-02-13", "daysLeft": 90,
  "reason": "Після останньої донації (15.11.2026) потрібно 90 дн." }
```

Центри поруч:

```http
GET /api/centers?lat=50.4501&lng=30.5234&limit=2
```

```json
200 OK
[
  { "id": 1, "name": "Центр крові «Поділ» (демо)", "address": "вул. Сагайдачного, 10", "city": "Київ",
    "latitude": 50.4627, "longitude": 30.5196, "phone": "+380440000001",
    "workingHours": "Пн–Пт 08:00–13:00", "distanceKm": 1.43 },
  { "id": 2, "name": "Центр крові «Лівобережний» (демо)", "...": "...", "distanceKm": 5.28 }
]
```

## Помилки

Усі помилки повертаються у форматі `application/problem+json` (RFC 7807):

```json
400 Bad Request
{ "status": 400, "title": "Донором можна стати з 18 років.", "errors": ["Донором можна стати з 18 років."] }
```

## Правила калькулятора

| Остання донація → наступна | Мінімальний інтервал |
| --- | --- |
| Цільна кров → цільна кров | 60 днів (чоловіки), 90 днів (жінки або стать не вказана) |
| Цільна кров → плазма / тромбоцити | 30 днів |
| Плазма / тромбоцити → будь-яка | 14 днів |
| Річний ліміт цільної крові | 5 (чоловіки), 4 (жінки) за 365 днів |

Значення зібрані в `DonationRules` і потребують звірки з чинним наказом МОЗ України перед пілотом.
