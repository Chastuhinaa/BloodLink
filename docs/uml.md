# UML-моделі системи (B-12)

Діаграми написані в Mermaid: GitHub показує їх прямо в цьому файлі, а змінюються вони разом із кодом у PR.

## 1. Діаграма варіантів використання

```mermaid
flowchart LR
    Donor(["👤 Донор"])
    Seeker(["👤 Людина, якій<br/>потрібна кров"])
    Guest(["👤 Гість"])
    Center(["🏥 Центр крові<br/>(весна)"])

    subgraph BL["BloodLink — модуль 2"]
        UC1(("Зареєструватися"))
        UC2(("Увійти"))
        UC3(("Заповнити профіль:<br/>група, резус, стать, вік, місце"))
        UC4(("Дізнатися, коли<br/>можна здавати"))
        UC5(("Знайти найближчий<br/>центр крові"))
        UC6(("Додати донацію<br/>в історію"))
        UC7(("Переглянути / видалити<br/>запис історії"))
        UC10(("Створити запит<br/>на кров"))
        UC11(("Побачити донорів,<br/>що відгукнулися"))
        UC12(("Закрити запит:<br/>знайдено / скасовано"))
        UC13(("Переглянути запити<br/>поруч"))
        UC14(("Відгукнутися<br/>«Я допоможу»"))
    end

    subgraph Spring["Весна (поза модулем 2)"]
        UC8(("Push-сповіщення<br/>про новий запит"))
        UC9(("Запит від імені<br/>центру крові"))
    end

    Guest --> UC1
    Guest --> UC2
    Guest --> UC5
    Donor --> UC3
    Donor --> UC4
    Donor --> UC5
    Donor --> UC6
    Donor --> UC7
    Donor --> UC13
    Donor --> UC14
    Seeker --> UC10
    Seeker --> UC11
    Seeker --> UC12
    Donor -.-> UC8
    Center -.-> UC9
    UC4 -. "«include»" .-> UC6
    UC14 -. "«include»" .-> UC4
```

Гість може зареєструватися, увійти й переглянути центри (ендпоінт `/api/centers` публічний). Решта дій потребує входу. Калькулятор (UC4) рахує дату за історією донацій (UC6). Донор і людина, якій потрібна кров, — це ролі одного облікового запису: родич пацієнта може сам бути донором. Відгукнутися (UC14) можна лише за сумісної групи й після мінімального інтервалу, тому UC14 включає перевірку UC4.

## 2. Діаграма класів

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
        +DateTime CreatedAt
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
    class DonationIntervalCalculator {
        +Calculate(sex, history, next, today) EligibilityResult
    }
    class DonationRules {
        +MinIntervalDays(previous, next, sex) int
        +MaxWholeBloodPerYear(sex) int
    }
    class EligibilityResult {
        <<record>>
        +bool CanDonateNow
        +DateOnly NextDate
        +int DaysLeft
        +string Reason
    }
    class GeoDistance {
        <<static>>
        +Kilometers(lat1, lon1, lat2, lon2) double
    }
    class ProfileValidator {
        <<static>>
        +Validate(...) IReadOnlyList~string~
        +AgeOn(birthDate, today) int
        +FormatBloodType(group, rh) string
    }

    class IAuthService { <<interface>> +RegisterAsync() +LoginAsync() }
    class IProfileService { <<interface>> +GetAsync() +UpdateAsync() }
    class IEligibilityService { <<interface>> +GetAsync() }
    class ICenterService { <<interface>> +GetAsync() }
    class IDonationService { <<interface>> +ListAsync() +AddAsync() +DeleteAsync() }
    class ITokenService { <<interface>> +CreateToken(donor) }
    class IBloodRequestService { <<interface>> +ListOpenAsync() +ListMineAsync() +CreateAsync() +RespondAsync() +CancelResponseAsync() +UpdateStatusAsync() }
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
    class BloodCompatibility {
        <<static>>
        +CanDonate(donorGroup, donorRh, recipientGroup, recipientRh, type) bool
        +CompatibleDonors(recipientGroup, recipientRh, type) list
    }
    class BloodLinkDbContext {
        +DbSet~Donor~ Donors
        +DbSet~Donation~ Donations
        +DbSet~BloodCenter~ BloodCenters
        +DbSet~BloodRequest~ BloodRequests
        +DbSet~RequestResponse~ RequestResponses
    }

    Donor "1" --> "0..*" Donation : має
    Donation "0..*" --> "0..1" BloodCenter : у центрі
    DonationIntervalCalculator --> DonationRules
    DonationIntervalCalculator ..> EligibilityResult : створює
    DonationIntervalCalculator ..> Donation : читає

    IAuthService ..> ITokenService
    IAuthService ..> BloodLinkDbContext
    IProfileService ..> ProfileValidator
    IProfileService ..> BloodLinkDbContext
    IEligibilityService ..> DonationIntervalCalculator
    IEligibilityService ..> BloodLinkDbContext
    ICenterService ..> GeoDistance
    ICenterService ..> BloodLinkDbContext
    IDonationService ..> BloodLinkDbContext
    Donor "1" --> "0..*" BloodRequest : створює
    BloodRequest "0..*" --> "1" BloodCenter : здавати тут
    BloodRequest "1" --> "0..*" RequestResponse : відгуки
    Donor "1" --> "0..*" RequestResponse : відгукується
    IBloodRequestService ..> BloodCompatibility
    IBloodRequestService ..> DonationIntervalCalculator
    IBloodRequestService ..> GeoDistance
    IBloodRequestService ..> BloodLinkDbContext
```

Переліки: `BloodGroup {O, A, B, AB}`, `RhFactor {Positive, Negative}`, `Sex {Male, Female}`, `DonationType {WholeBlood, Plasma, Platelets}`, `RequestUrgency {Normal, Urgent, Critical}`, `RequestStatus {Open, Fulfilled, Cancelled}`. Кожен інтерфейс сервісу має одну реалізацію з такою ж назвою без `I` (`AuthService`, `ProfileService` тощо); контролери залежать лише від інтерфейсів.

## 3. Діаграма послідовності: «коли мені можна здавати?»

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

## 4. Діаграма послідовності: реєстрація і вхід

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

## 5. Діаграма послідовності: запит на кров і відгук донора

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
