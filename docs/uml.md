# UML-моделі системи (B-12)

Діаграми написані в Mermaid: GitHub показує їх прямо в цьому файлі, а змінюються вони разом із кодом у PR.

## 1. Діаграма варіантів використання

```mermaid
flowchart LR
    Donor(["👤 Донор"])
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
    end

    subgraph Spring["Весна (поза модулем 2)"]
        UC8(("Отримати SOS-<br/>сповіщення"))
        UC9(("Створити SOS-запит"))
    end

    Guest --> UC1
    Guest --> UC2
    Guest --> UC5
    Donor --> UC3
    Donor --> UC4
    Donor --> UC5
    Donor --> UC6
    Donor --> UC7
    Donor -.-> UC8
    Center -.-> UC9
    UC4 -. "«include»" .-> UC6
```

Гість може зареєструватися, увійти й переглянути центри (ендпоінт `/api/centers` публічний). Решта дій потребує входу. Калькулятор (UC4) рахує дату за історією донацій (UC6).

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
    class BloodLinkDbContext {
        +DbSet~Donor~ Donors
        +DbSet~Donation~ Donations
        +DbSet~BloodCenter~ BloodCenters
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
```

Переліки: `BloodGroup {O, A, B, AB}`, `RhFactor {Positive, Negative}`, `Sex {Male, Female}`, `DonationType {WholeBlood, Plasma, Platelets}`. Кожен інтерфейс сервісу має одну реалізацію з такою ж назвою без `I` (`AuthService`, `ProfileService` тощо); контролери залежать лише від інтерфейсів.

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
