# UML-моделі системи (B-12)

Діаграми написані в Mermaid: GitHub показує їх прямо в цьому файлі, а змінюються вони разом із кодом у PR.

## 1. Діаграма варіантів використання

```mermaid
flowchart LR
    Guest(["👤 Гість"])
    Donor(["👤 Донор"])

    subgraph BL["BloodLink"]
        direction TB
        UC1(["Зареєструватися / увійти"])
        UC5(["Знайти найближчий центр крові"])
        UC3(["Заповнити профіль: група, резус, стать, вік, місце"])
        UC4(["Дізнатися, коли можна здавати"])
        UC6(["Вести історію донацій"])
        UC13(["Переглянути запити на кров поруч"])
        UC14(["Відгукнутися «Я допоможу»"])
        UC10(["Створити запит на кров"])
        UC11(["Побачити донорів, що відгукнулися"])
        UC12(["Закрити запит: знайдено / скасовано"])
    end

    subgraph Spring["Весна"]
        direction TB
        UC8(["Push-сповіщення про новий запит"])
        UC9(["Запит від імені центру крові"])
    end

    Seeker(["👤 Людина, якій<br/>потрібна кров"])
    Center(["🏥 Центр крові"])

    Guest --> UC1
    Guest --> UC5
    Donor --> UC3
    Donor --> UC4
    Donor --> UC6
    Donor --> UC5
    Donor --> UC13
    Donor --> UC14
    Donor -.-> UC8
    UC10 --- Seeker
    UC11 --- Seeker
    UC12 --- Seeker
    UC9 -.- Center
    UC4 -. "«include»" .-> UC6
    UC14 -. "«include»" .-> UC4
```

Гість може зареєструватися, увійти й переглянути центри (`/api/centers` — публічний ендпоінт); решта дій потребує входу. Донор і людина, якій потрібна кров, — ролі одного облікового запису: родич пацієнта може сам бути донором. Калькулятор (UC4) рахує дату за історією донацій (UC6). Відгукнутися (UC14) можна лише за сумісної групи й після мінімального інтервалу, тому UC14 включає UC4. Пунктиром — функції, заплановані на весну.

## 2. Діаграма класів: модель даних

```mermaid
classDiagram
    direction LR

    class Donor {
        +int Id
        +string Email
        +string PasswordHash
        +string FullName
        +Sex? Sex
        +DateOnly? BirthDate
        +BloodGroup? BloodGroup
        +RhFactor? RhFactor
        +string? City
        +double? Latitude
        +double? Longitude
    }
    class Donation {
        +int Id
        +int DonorId
        +int? BloodCenterId
        +DateOnly Date
        +DonationType Type
        +string? Notes
    }
    class BloodCenter {
        +int Id
        +string Name
        +string Address
        +string City
        +double Latitude
        +double Longitude
        +string? Phone
        +string? WorkingHours
    }
    class BloodRequest {
        +int Id
        +int AuthorId
        +BloodGroup BloodGroup
        +RhFactor RhFactor
        +DonationType Type
        +int UnitsNeeded
        +int BloodCenterId
        +RequestUrgency Urgency
        +RequestStatus Status
        +string ContactPhone
        +string? PatientName
        +string? Hospital
        +DateOnly? NeededBy
    }
    class RequestResponse {
        +int Id
        +int RequestId
        +int DonorId
        +DateTime CreatedAt
    }

    Donor "1" --> "0..*" Donation : має
    Donation "0..*" --> "0..1" BloodCenter : у центрі
    Donor "1" --> "0..*" BloodRequest : створює
    BloodRequest "0..*" --> "1" BloodCenter : здавати тут
    BloodRequest "1" *-- "0..*" RequestResponse : відгуки
    Donor "1" --> "0..*" RequestResponse : відгукується
```

Переліки: `BloodGroup {O, A, B, AB}`, `RhFactor {Positive, Negative}`, `Sex {Male, Female}`, `DonationType {WholeBlood, Plasma, Platelets}`, `RequestUrgency {Normal, Urgent, Critical}`, `RequestStatus {Open, Fulfilled, Cancelled}`.

## 3. Діаграма класів: сервіси та бізнес-логіка

```mermaid
classDiagram
    direction LR

    class IAuthService {
        <<interface>>
        +RegisterAsync(request) AuthResponse
        +LoginAsync(request) AuthResponse
    }
    class IProfileService {
        <<interface>>
        +GetAsync(donorId) ProfileDto
        +UpdateAsync(donorId, request) ProfileDto
    }
    class IEligibilityService {
        <<interface>>
        +GetAsync(donorId, type) EligibilityDto
    }
    class ICenterService {
        <<interface>>
        +GetAsync(lat, lng, limit) List~CenterDto~
    }
    class IDonationService {
        <<interface>>
        +ListAsync(donorId) List~DonationDto~
        +AddAsync(donorId, request) DonationDto
        +DeleteAsync(donorId, id)
    }
    class IBloodRequestService {
        <<interface>>
        +ListOpenAsync(userId, lat, lng, radius, onlyCompatible)
        +ListMineAsync(userId)
        +CreateAsync(userId, request)
        +RespondAsync(userId, requestId)
        +UpdateStatusAsync(userId, requestId, status)
    }
    class ITokenService {
        <<interface>>
        +CreateToken(donor) string
    }
    class DonationIntervalCalculator {
        +Calculate(sex, history, next, today) EligibilityResult
    }
    class DonationRules {
        +MinIntervalDays(previous, next, sex) int
        +MaxWholeBloodPerYear(sex) int
    }
    class BloodCompatibility {
        <<static>>
        +CanDonate(donor, recipient, type) bool
    }
    class GeoDistance {
        <<static>>
        +Kilometers(lat1, lon1, lat2, lon2) double
    }
    class ProfileValidator {
        <<static>>
        +Validate(...) List~string~
    }
    IAuthService ..> ITokenService
    IProfileService ..> ProfileValidator
    IEligibilityService ..> DonationIntervalCalculator
    ICenterService ..> GeoDistance
    IBloodRequestService ..> BloodCompatibility
    IBloodRequestService ..> DonationIntervalCalculator
    IBloodRequestService ..> GeoDistance
    DonationIntervalCalculator --> DonationRules
```

Кожен інтерфейс має одну реалізацію з такою ж назвою без `I` (`AuthService`, `BloodRequestService` тощо); контролери залежать лише від інтерфейсів. Усі сервіси читають і записують дані через `BloodLinkDbContext` (EF Core) — ці зв'язки на діаграмі не показані, щоб не перевантажувати її. Бізнес-правила (`DonationIntervalCalculator`, `BloodCompatibility`, `GeoDistance`, `ProfileValidator`) лежать у проєкті `BloodLink.Domain` і не залежать від веба чи БД.

## 4. Діаграма послідовності: «коли мені можна здавати?»

```mermaid
sequenceDiagram
    actor D as Донор
    participant UI as Клієнт
    participant API as EligibilityController
    participant JWT as JwtBearer middleware
    participant S as EligibilityService
    participant DB as BloodLinkDbContext
    participant Calc as DonationIntervalCalculator

    D->>UI: відкриває екран «Профіль»
    UI->>API: GET /api/eligibility?type=WholeBlood<br/>Authorization: Bearer <token>
    API->>JWT: перевірка підпису й терміну дії
    alt токен недійсний
        JWT-->>UI: 401 Unauthorized
        UI-->>D: екран входу
    else токен дійсний
        API->>S: GetAsync(donorId із claim "sub", type)
        S->>DB: донор + його донації
        DB-->>S: Donor, List<Donation>
        S->>Calc: Calculate(sex, history, type, today)
        Calc-->>S: EligibilityResult
        S-->>API: EligibilityDto
        API-->>UI: 200 OK { canDonateNow, nextDate, daysLeft, reason }
        UI-->>D: «Через 90 дн. — з 13.02.2027»
    end
```

## 5. Діаграма послідовності: реєстрація і вхід

```mermaid
sequenceDiagram
    actor D as Донор
    participant UI as Клієнт
    participant A as AuthController
    participant S as AuthService
    participant H as PasswordHasher
    participant DB as BloodLinkDbContext
    participant T as JwtTokenService

    D->>UI: email, пароль, ім'я
    UI->>A: POST /api/auth/register
    A->>A: валідація DTO (email, пароль ≥ 8)
    A->>S: RegisterAsync(request)
    S->>DB: чи є такий email?
    alt email зайнятий
        S-->>UI: 409 Conflict (problem+json)
    else новий донор
        S->>H: HashPassword(password)
        S->>DB: INSERT donor
        S->>T: CreateToken(donor)
        T-->>S: JWT + термін дії
        S-->>A: AuthResponse
        A-->>UI: 201 Created { token, expiresAt, donorId }
        UI->>UI: зберегти токен, перейти на «Профіль»
    end
```

## 6. Діаграма послідовності: запит на кров і відгук донора

```mermaid
sequenceDiagram
    actor R as Родич пацієнта
    actor D as Донор
    participant API as BloodRequestsController
    participant S as BloodRequestService
    participant BC as BloodCompatibility
    participant Calc as DonationIntervalCalculator
    participant DB as BloodLinkDbContext

    R->>API: POST /api/requests { A Rh−, 3 донори, центр 1, Critical, телефон }
    API->>S: CreateAsync(authorId, request)
    S->>DB: INSERT blood_requests
    API-->>R: 201 Created

    D->>API: GET /api/requests?onlyCompatible=true
    API->>S: ListOpenAsync(donorId, ...)
    S->>DB: відкриті запити + центр + профіль донора
    loop для кожного запиту
        S->>BC: CanDonate(група донора, група пацієнта, тип)
        S->>Calc: Calculate(історія донора)
    end
    API-->>D: 200 [{ canHelp: true, contactPhone: null, distanceKm: 1.4 }]

    D->>API: POST /api/requests/{id}/responses
    API->>S: RespondAsync(donorId, id)
    alt група несумісна або ще не минув інтервал
        S-->>D: 400 + причина
    else можна допомогти
        S->>DB: INSERT request_responses
        API-->>D: 200 { iResponded: true, contactPhone: "+380…" }
    end

    R->>API: GET /api/requests/mine
    API-->>R: 200 [{ responders: [{ ім'я, група, email }] }]
    R->>API: PATCH /api/requests/{id}/status { Fulfilled }
    API-->>R: 200, запит зникає зі списку відкритих
```
