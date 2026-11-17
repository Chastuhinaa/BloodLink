# Схема бази даних (B-01)

**Коротко:** п'ять таблиць — користувачі (донори), центри крові, донації, запити на кров і відгуки донорів на запити. Один обліковий запис може і здавати кров, і шукати її для близької людини. Схему описує `BloodLinkDbContext` (EF Core), тож вона однакова для PostgreSQL і SQLite.

## ER-діаграма

```mermaid
erDiagram
    donors ||--o{ donations : "має"
    blood_centers |o--o{ donations : "у центрі"
    donors ||--o{ blood_requests : "створює"
    blood_centers ||--o{ blood_requests : "здавати тут"
    blood_requests ||--o{ request_responses : "має відгуки"
    donors ||--o{ request_responses : "відгукується"

    donors {
        int Id PK
        varchar(254) Email UK "унікальний, у нижньому регістрі"
        text PasswordHash "PBKDF2"
        varchar(100) FullName
        varchar(10) Sex "Male | Female, NULL"
        date BirthDate "NULL"
        varchar(3) BloodGroup "O | A | B | AB, NULL"
        varchar(10) RhFactor "Positive | Negative, NULL"
        varchar(100) City "NULL"
        float Latitude "NULL"
        float Longitude "NULL"
        timestamp CreatedAt
    }

    blood_centers {
        int Id PK
        varchar(200) Name
        varchar(300) Address
        varchar(100) City
        float Latitude
        float Longitude
        varchar(30) Phone "NULL"
        varchar(100) WorkingHours "NULL"
    }

    donations {
        int Id PK
        int DonorId FK "ON DELETE CASCADE"
        int BloodCenterId FK "NULL, ON DELETE SET NULL"
        date Date
        varchar(20) Type "WholeBlood | Plasma | Platelets"
        varchar(500) Notes "NULL"
    }

    blood_requests {
        int Id PK
        int AuthorId FK "ON DELETE CASCADE"
        int BloodCenterId FK "ON DELETE RESTRICT"
        varchar(3) BloodGroup "O | A | B | AB"
        varchar(10) RhFactor "Positive | Negative"
        varchar(20) Type "WholeBlood | Plasma | Platelets"
        int UnitsNeeded "1..20"
        varchar(10) Urgency "Normal | Urgent | Critical"
        varchar(10) Status "Open | Fulfilled | Cancelled"
        varchar(20) ContactPhone
        varchar(100) PatientName "NULL, ініціали"
        varchar(200) Hospital "NULL"
        varchar(500) Description "NULL"
        date NeededBy "NULL"
        timestamp CreatedAt
    }

    request_responses {
        int Id PK
        int RequestId FK "ON DELETE CASCADE"
        int DonorId FK "ON DELETE CASCADE"
        timestamp CreatedAt
    }
```

## Обмеження й індекси

| Таблиця | Обмеження | Навіщо |
| --- | --- | --- |
| `donors` | `UNIQUE (Email)` | Один обліковий запис на email; email нормалізується до нижнього регістру. |
| `donations` | `FK DonorId → donors.Id ON DELETE CASCADE` | Видалення донора видаляє і його історію. |
| `donations` | `FK BloodCenterId → blood_centers.Id ON DELETE SET NULL` | Закриття центру не знищує історію донора. |
| `donations` | `INDEX (DonorId, Date)` | Швидка вибірка історії донора і останньої донації для калькулятора. |
| `blood_requests` | `FK BloodCenterId → blood_centers.Id ON DELETE RESTRICT` | Не можна видалити центр, поки на нього посилаються запити. |
| `blood_requests` | `INDEX (Status, BloodGroup, RhFactor)` | Швидкий пошук відкритих запитів потрібної групи. |
| `request_responses` | `UNIQUE (RequestId, DonorId)` | Донор відгукується на запит лише один раз. |

Переліки зберігаються як рядки (`HasConversion<string>()`): у БД видно `"WholeBlood"`, а не `1`, і додавання нового значення не зсуває старі.

## Початкові дані

П'ять демо-центрів крові в Києві (`SeedData.Centers`) — назви й координати умовні й позначені «(демо)». Наповнення бази реальними центрами — окрема задача беклогу.

## Плани на весну

- PostGIS-колонка `geography(Point)` замість пари `Latitude/Longitude` і просторовий індекс — для SOS у радіусі 15 км.
- Таблиця `notifications` для push-сповіщень про нові запити, обліковий запис центру крові.
- Міграції EF Core (`dotnet ef migrations add`) замість `EnsureCreated`.
