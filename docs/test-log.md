# Журнал результатів тестування (B-09, B-10)

**Підсумок:** модульні — 41 з 41 пройшли, 0 не пройшли; інтеграційні — 13 з 13 пройшли, 0 не пройшли. Прогін: 2026-11-15 16:59:07 (UTC).

Журнал згенеровано з `.trx`-файлів командою `python tools/trx_to_md.py` — не редагуйте його вручну, а перегенеруйте після змін. Кожен випадок `[Theory]` з `InlineData` рахується окремим тестом.

## Модульні тести за учасницями

| Учасниця | Кількість тестів |
| --- | --- |
| Андрущенко Альона | 12 |
| Дорошенко Дарія | 12 |
| Лисенко Катерина | 9 |
| Частухіна Юлія | 8 |

## Модульні тести (`tests/BloodLink.UnitTests`)

| № | Клас | Тест | Задача | Автор | Результат | Час, мс |
| --- | --- | --- | --- | --- | --- | --- |
| 1 | AuthTests | `Constructor_ShortKey_Throws` | B-03 | Частухіна Юлія | ✅ пройшов | 10 |
| 2 | AuthTests | `CreateToken_ContainsDonorIdAndEmail` | B-03 | Частухіна Юлія | ✅ пройшов | 32 |
| 3 | AuthTests | `CreateToken_ExpiresAfterConfiguredMinutes` | B-03 | Частухіна Юлія | ✅ пройшов | 1296 |
| 4 | AuthTests | `Login_CorrectPassword_ReturnsToken` | B-03 | Частухіна Юлія | ✅ пройшов | 243 |
| 5 | AuthTests | `Login_WrongCredentials_Unauthorized(email: "login@test.ua", password: "WrongPass1")` | B-03 | Частухіна Юлія | ✅ пройшов | 255 |
| 6 | AuthTests | `Login_WrongCredentials_Unauthorized(email: "nobody@test.ua", password: "Passw0rd!")` | B-03 | Частухіна Юлія | ✅ пройшов | 531 |
| 7 | AuthTests | `Register_DuplicateEmail_Conflict` | B-03 | Частухіна Юлія | ✅ пройшов | 134 |
| 8 | AuthTests | `Register_StoresHashedPasswordAndNormalizedEmail` | B-03 | Частухіна Юлія | ✅ пройшов | 146 |
| 9 | DonationIntervalCalculatorTests | `ExactlyOnIntervalBoundary_CanDonate` | B-05 | Лисенко Катерина | ✅ пройшов | 1 |
| 10 | DonationIntervalCalculatorTests | `Female_WholeBlood_Waits90Days` | B-05 | Лисенко Катерина | ✅ пройшов | 0 |
| 11 | DonationIntervalCalculatorTests | `FutureDonations_AreIgnored` | B-05 | Лисенко Катерина | ✅ пройшов | 0 |
| 12 | DonationIntervalCalculatorTests | `Male_WholeBlood_Waits60Days` | B-05 | Лисенко Катерина | ✅ пройшов | 6 |
| 13 | DonationIntervalCalculatorTests | `NoHistory_CanDonateToday` | B-05 | Лисенко Катерина | ✅ пройшов | 0 |
| 14 | DonationIntervalCalculatorTests | `Plasma_After_Plasma_Waits14Days` | B-05 | Лисенко Катерина | ✅ пройшов | 0 |
| 15 | DonationIntervalCalculatorTests | `UnknownSex_UsesStricterInterval` | B-05 | Лисенко Катерина | ✅ пройшов | 22 |
| 16 | DonationIntervalCalculatorTests | `UsesLatestDonation_RegardlessOfOrder` | B-05 | Лисенко Катерина | ✅ пройшов | 4 |
| 17 | DonationIntervalCalculatorTests | `YearlyLimit_Male_BlocksSixthWholeBloodDonation` | B-05 | Лисенко Катерина | ✅ пройшов | 22 |
| 18 | DonationServiceTests | `Add_FutureDate_Rejected` | B-07 | Дорошенко Дарія | ✅ пройшов | 20 |
| 19 | DonationServiceTests | `Add_ThenList_ReturnsNewestFirst` | B-07 | Дорошенко Дарія | ✅ пройшов | 1888 |
| 20 | DonationServiceTests | `Add_UnknownCenter_Rejected` | B-07 | Дорошенко Дарія | ✅ пройшов | 17 |
| 21 | DonationServiceTests | `Delete_OtherDonorsRecord_NotFound` | B-07 | Дорошенко Дарія | ✅ пройшов | 30 |
| 22 | GeoAndCentersTests | `Distance_InvalidCoordinates_Throws(lat: 50, lon: 181)` | B-06 | Дорошенко Дарія | ✅ пройшов | 1 |
| 23 | GeoAndCentersTests | `Distance_InvalidCoordinates_Throws(lat: 91, lon: 30)` | B-06 | Дорошенко Дарія | ✅ пройшов | 1 |
| 24 | GeoAndCentersTests | `Distance_IsSymmetric` | B-06 | Дорошенко Дарія | ✅ пройшов | 1 |
| 25 | GeoAndCentersTests | `Distance_KyivToLviv_IsAbout470Km` | B-06 | Дорошенко Дарія | ✅ пройшов | 3 |
| 26 | GeoAndCentersTests | `Distance_SamePoint_IsZero` | B-06 | Дорошенко Дарія | ✅ пройшов | 0 |
| 27 | GeoAndCentersTests | `SortByDistance_NearestFirst` | B-06 | Дорошенко Дарія | ✅ пройшов | 13 |
| 28 | GeoAndCentersTests | `SortByDistance_RespectsLimit` | B-06 | Дорошенко Дарія | ✅ пройшов | 1 |
| 29 | GeoAndCentersTests | `SortByDistance_WithoutCoordinates_SortsByNameWithoutDistance` | B-06 | Дорошенко Дарія | ✅ пройшов | 34 |
| 30 | ProfileTests | `AgeOn_BeforeBirthday_CountsPreviousYear` | B-04 | Андрущенко Альона | ✅ пройшов | 9 |
| 31 | ProfileTests | `FormatBloodType_ReturnsUkrainianNotation(g: A, rh: Negative, expected: "A(II) Rh−")` | B-04 | Андрущенко Альона | ✅ пройшов | 9 |
| 32 | ProfileTests | `FormatBloodType_ReturnsUkrainianNotation(g: O, rh: Positive, expected: "0(I) Rh+")` | B-04 | Андрущенко Альона | ✅ пройшов | 110 |
| 33 | ProfileTests | `FormatBloodType_ReturnsUkrainianNotation(g: null, rh: null, expected: "не вказано")` | B-04 | Андрущенко Альона | ✅ пройшов | 1256 |
| 34 | ProfileTests | `UpdateAsync_SavesFieldsAndTrimsCity` | B-04 | Андрущенко Альона | ✅ пройшов | 341 |
| 35 | ProfileTests | `UpdateAsync_UnknownDonor_NotFound` | B-04 | Андрущенко Альона | ✅ пройшов | 21 |
| 36 | ProfileTests | `Validate_AgeOutOfRange_Error(y: 1960, m: 1, d: 1)` | B-04 | Андрущенко Альона | ✅ пройшов | 11 |
| 37 | ProfileTests | `Validate_AgeOutOfRange_Error(y: 2009, m: 1, d: 1)` | B-04 | Андрущенко Альона | ✅ пройшов | 9 |
| 38 | ProfileTests | `Validate_AgeOutOfRange_Error(y: 2027, m: 1, d: 1)` | B-04 | Андрущенко Альона | ✅ пройшов | 10 |
| 39 | ProfileTests | `Validate_CorrectProfile_NoErrors` | B-04 | Андрущенко Альона | ✅ пройшов | 12 |
| 40 | ProfileTests | `Validate_EmptyName_Error` | B-04 | Андрущенко Альона | ✅ пройшов | 10 |
| 41 | ProfileTests | `Validate_OnlyLatitude_Error` | B-04 | Андрущенко Альона | ✅ пройшов | 11 |

## Інтеграційні тести (`tests/BloodLink.IntegrationTests`)

Застосунок піднімається в пам'яті через `WebApplicationFactory<Program>` з окремою SQLite-базою; тести шлють справжні HTTP-запити й проходять маршрутизацію, JWT, валідацію і EF Core.

| № | Клас | Тест | Задача | Автор | Результат | Час, мс |
| --- | --- | --- | --- | --- | --- | --- |
| 1 | ApiTests | `Centers_OnlyLatitude_Returns400` | B-10 | Частухіна Юлія | ✅ пройшов | 32 |
| 2 | ApiTests | `DemoScenario_Register_Profile_Eligibility_Centers_History` | B-10 | Частухіна Юлія | ✅ пройшов | 338 |
| 3 | ApiTests | `Donations_AreIsolatedBetweenDonors` | B-10 | Частухіна Юлія | ✅ пройшов | 449 |
| 4 | ApiTests | `Donations_FutureDate_Returns400` | B-10 | Частухіна Юлія | ✅ пройшов | 273 |
| 5 | ApiTests | `Login_AfterRegister_Returns200_AndWrongPassword401` | B-10 | Частухіна Юлія | ✅ пройшов | 3024 |
| 6 | ApiTests | `Profile_Underage_Returns400WithErrors` | B-10 | Частухіна Юлія | ✅ пройшов | 156 |
| 7 | ApiTests | `ProtectedEndpoint_WithForgedToken_Returns401` | B-10 | Частухіна Юлія | ✅ пройшов | 41 |
| 8 | ApiTests | `ProtectedEndpoints_WithoutToken_Return401(url: "/api/donations")` | B-10 | Частухіна Юлія | ✅ пройшов | 3 |
| 9 | ApiTests | `ProtectedEndpoints_WithoutToken_Return401(url: "/api/eligibility")` | B-10 | Частухіна Юлія | ✅ пройшов | 4 |
| 10 | ApiTests | `ProtectedEndpoints_WithoutToken_Return401(url: "/api/profile")` | B-10 | Частухіна Юлія | ✅ пройшов | 4 |
| 11 | ApiTests | `Register_DuplicateEmail_Returns409` | B-10 | Частухіна Юлія | ✅ пройшов | 130 |
| 12 | ApiTests | `Register_ShortPassword_Returns400` | B-10 | Частухіна Юлія | ✅ пройшов | 16 |
| 13 | ApiTests | `Swagger_DocumentDescribesAllEndpoints` | B-10 | Частухіна Юлія | ✅ пройшов | 195 |
