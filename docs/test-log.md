# Журнал результатів тестування (B-09, B-10)

**Підсумок:** модульні — 60 з 60 пройшли, 0 не пройшли; інтеграційні — 16 з 16 пройшли, 0 не пройшли. Прогін: 2026-11-15 17:30:53 (UTC).

Журнал згенеровано з `.trx`-файлів командою `python tools/trx_to_md.py` — не редагуйте його вручну, а перегенеруйте після змін. Кожен випадок `[Theory]` з `InlineData` рахується окремим тестом.

## Модульні тести за учасницями

| Учасниця | Кількість тестів |
| --- | --- |
| Андрущенко Альона | 12 |
| Дорошенко Дарія | 12 |
| Лисенко Катерина | 9 |
| Частухіна Юлія | 27 |

## Модульні тести (`tests/BloodLink.UnitTests`)

| № | Клас | Тест | Задача | Автор | Результат | Час, мс |
| --- | --- | --- | --- | --- | --- | --- |
| 1 | AuthTests | `Constructor_ShortKey_Throws` | B-03 | Частухіна Юлія | ✅ пройшов | 21 |
| 2 | AuthTests | `CreateToken_ContainsDonorIdAndEmail` | B-03 | Частухіна Юлія | ✅ пройшов | 39 |
| 3 | AuthTests | `CreateToken_ExpiresAfterConfiguredMinutes` | B-03 | Частухіна Юлія | ✅ пройшов | 1461 |
| 4 | AuthTests | `Login_CorrectPassword_ReturnsToken` | B-03 | Частухіна Юлія | ✅ пройшов | 261 |
| 5 | AuthTests | `Login_WrongCredentials_Unauthorized(email: "login@test.ua", password: "WrongPass1")` | B-03 | Частухіна Юлія | ✅ пройшов | 295 |
| 6 | AuthTests | `Login_WrongCredentials_Unauthorized(email: "nobody@test.ua", password: "Passw0rd!")` | B-03 | Частухіна Юлія | ✅ пройшов | 629 |
| 7 | AuthTests | `Register_DuplicateEmail_Conflict` | B-03 | Частухіна Юлія | ✅ пройшов | 137 |
| 8 | AuthTests | `Register_StoresHashedPasswordAndNormalizedEmail` | B-03 | Частухіна Юлія | ✅ пройшов | 153 |
| 9 | BloodCompatibilityTests | `CompatibleDonors_ForAbPositive_AllEightTypes` | B-28 | Частухіна Юлія | ✅ пройшов | 6 |
| 10 | BloodCompatibilityTests | `CompatibleDonors_ForONegative_OnlyONegative` | B-28 | Частухіна Юлія | ✅ пройшов | 7 |
| 11 | BloodCompatibilityTests | `Plasma_AbIsUniversalDonor_AndRhIgnored` | B-28 | Частухіна Юлія | ✅ пройшов | 8 |
| 12 | BloodCompatibilityTests | `WholeBlood_FollowsAboAndRhRules(dg: A, drh: Negative, rg: B, rrh: Negative, expected: False)` | B-28 | Частухіна Юлія | ✅ пройшов | 0 |
| 13 | BloodCompatibilityTests | `WholeBlood_FollowsAboAndRhRules(dg: A, drh: Positive, rg: A, rrh: Negative, expected: False)` | B-28 | Частухіна Юлія | ✅ пройшов | 0 |
| 14 | BloodCompatibilityTests | `WholeBlood_FollowsAboAndRhRules(dg: AB, drh: Positive, rg: O, rrh: Positive, expected: False)` | B-28 | Частухіна Юлія | ✅ пройшов | 1 |
| 15 | BloodCompatibilityTests | `WholeBlood_FollowsAboAndRhRules(dg: B, drh: Positive, rg: AB, rrh: Positive, expected: True)` | B-28 | Частухіна Юлія | ✅ пройшов | 5 |
| 16 | BloodCompatibilityTests | `WholeBlood_FollowsAboAndRhRules(dg: O, drh: Negative, rg: A, rrh: Negative, expected: True)` | B-28 | Частухіна Юлія | ✅ пройшов | 0 |
| 17 | BloodCompatibilityTests | `WholeBlood_FollowsAboAndRhRules(dg: O, drh: Negative, rg: AB, rrh: Positive, expected: True)` | B-28 | Частухіна Юлія | ✅ пройшов | 0 |
| 18 | BloodRequestServiceTests | `Create_InvalidPhone_Rejected(phone: "123")` | B-28 | Частухіна Юлія | ✅ пройшов | 19 |
| 19 | BloodRequestServiceTests | `Create_InvalidPhone_Rejected(phone: "phone-number")` | B-28 | Частухіна Юлія | ✅ пройшов | 21 |
| 20 | BloodRequestServiceTests | `Create_SavesRequest_AndAuthorSeesPhone` | B-28 | Частухіна Юлія | ✅ пройшов | 27 |
| 21 | BloodRequestServiceTests | `Create_UnknownCenter_Rejected` | B-28 | Частухіна Юлія | ✅ пройшов | 20 |
| 22 | BloodRequestServiceTests | `List_HidesPhone_AndMarksWhoCanHelp` | B-28 | Частухіна Юлія | ✅ пройшов | 25 |
| 23 | BloodRequestServiceTests | `List_SortsCriticalFirst_AndFiltersCompatible` | B-28 | Частухіна Юлія | ✅ пройшов | 48 |
| 24 | BloodRequestServiceTests | `Respond_DonorNotYetEligible_Rejected` | B-28 | Частухіна Юлія | ✅ пройшов | 28 |
| 25 | BloodRequestServiceTests | `Respond_IncompatibleOrOwnOrTwice_Rejected` | B-28 | Частухіна Юлія | ✅ пройшов | 34 |
| 26 | BloodRequestServiceTests | `Respond_RevealsPhone_AndAuthorSeesResponder` | B-28 | Частухіна Юлія | ✅ пройшов | 2325 |
| 27 | BloodRequestServiceTests | `UpdateStatus_OnlyAuthor_AndClosedRequestDisappearsFromList` | B-28 | Частухіна Юлія | ✅ пройшов | 78 |
| 28 | DonationIntervalCalculatorTests | `ExactlyOnIntervalBoundary_CanDonate` | B-05 | Лисенко Катерина | ✅ пройшов | 0 |
| 29 | DonationIntervalCalculatorTests | `Female_WholeBlood_Waits90Days` | B-05 | Лисенко Катерина | ✅ пройшов | 0 |
| 30 | DonationIntervalCalculatorTests | `FutureDonations_AreIgnored` | B-05 | Лисенко Катерина | ✅ пройшов | 0 |
| 31 | DonationIntervalCalculatorTests | `Male_WholeBlood_Waits60Days` | B-05 | Лисенко Катерина | ✅ пройшов | 4 |
| 32 | DonationIntervalCalculatorTests | `NoHistory_CanDonateToday` | B-05 | Лисенко Катерина | ✅ пройшов | 0 |
| 33 | DonationIntervalCalculatorTests | `Plasma_After_Plasma_Waits14Days` | B-05 | Лисенко Катерина | ✅ пройшов | 0 |
| 34 | DonationIntervalCalculatorTests | `UnknownSex_UsesStricterInterval` | B-05 | Лисенко Катерина | ✅ пройшов | 17 |
| 35 | DonationIntervalCalculatorTests | `UsesLatestDonation_RegardlessOfOrder` | B-05 | Лисенко Катерина | ✅ пройшов | 2 |
| 36 | DonationIntervalCalculatorTests | `YearlyLimit_Male_BlocksSixthWholeBloodDonation` | B-05 | Лисенко Катерина | ✅ пройшов | 16 |
| 37 | DonationServiceTests | `Add_FutureDate_Rejected` | B-07 | Дорошенко Дарія | ✅ пройшов | 21 |
| 38 | DonationServiceTests | `Add_ThenList_ReturnsNewestFirst` | B-07 | Дорошенко Дарія | ✅ пройшов | 2157 |
| 39 | DonationServiceTests | `Add_UnknownCenter_Rejected` | B-07 | Дорошенко Дарія | ✅ пройшов | 21 |
| 40 | DonationServiceTests | `Delete_OtherDonorsRecord_NotFound` | B-07 | Дорошенко Дарія | ✅ пройшов | 37 |
| 41 | GeoAndCentersTests | `Distance_InvalidCoordinates_Throws(lat: 50, lon: 181)` | B-06 | Дорошенко Дарія | ✅ пройшов | 1 |
| 42 | GeoAndCentersTests | `Distance_InvalidCoordinates_Throws(lat: 91, lon: 30)` | B-06 | Дорошенко Дарія | ✅ пройшов | 1 |
| 43 | GeoAndCentersTests | `Distance_IsSymmetric` | B-06 | Дорошенко Дарія | ✅ пройшов | 1 |
| 44 | GeoAndCentersTests | `Distance_KyivToLviv_IsAbout470Km` | B-06 | Дорошенко Дарія | ✅ пройшов | 3 |
| 45 | GeoAndCentersTests | `Distance_SamePoint_IsZero` | B-06 | Дорошенко Дарія | ✅ пройшов | 0 |
| 46 | GeoAndCentersTests | `SortByDistance_NearestFirst` | B-06 | Дорошенко Дарія | ✅ пройшов | 8 |
| 47 | GeoAndCentersTests | `SortByDistance_RespectsLimit` | B-06 | Дорошенко Дарія | ✅ пройшов | 1 |
| 48 | GeoAndCentersTests | `SortByDistance_WithoutCoordinates_SortsByNameWithoutDistance` | B-06 | Дорошенко Дарія | ✅ пройшов | 27 |
| 49 | ProfileTests | `AgeOn_BeforeBirthday_CountsPreviousYear` | B-04 | Андрущенко Альона | ✅ пройшов | 39 |
| 50 | ProfileTests | `FormatBloodType_ReturnsUkrainianNotation(g: A, rh: Negative, expected: "A(II) Rh−")` | B-04 | Андрущенко Альона | ✅ пройшов | 27 |
| 51 | ProfileTests | `FormatBloodType_ReturnsUkrainianNotation(g: O, rh: Positive, expected: "0(I) Rh+")` | B-04 | Андрущенко Альона | ✅ пройшов | 112 |
| 52 | ProfileTests | `FormatBloodType_ReturnsUkrainianNotation(g: null, rh: null, expected: "не вказано")` | B-04 | Андрущенко Альона | ✅ пройшов | 1421 |
| 53 | ProfileTests | `UpdateAsync_SavesFieldsAndTrimsCity` | B-04 | Андрущенко Альона | ✅ пройшов | 320 |
| 54 | ProfileTests | `UpdateAsync_UnknownDonor_NotFound` | B-04 | Андрущенко Альона | ✅ пройшов | 43 |
| 55 | ProfileTests | `Validate_AgeOutOfRange_Error(y: 1960, m: 1, d: 1)` | B-04 | Андрущенко Альона | ✅ пройшов | 25 |
| 56 | ProfileTests | `Validate_AgeOutOfRange_Error(y: 2009, m: 1, d: 1)` | B-04 | Андрущенко Альона | ✅ пройшов | 59 |
| 57 | ProfileTests | `Validate_AgeOutOfRange_Error(y: 2027, m: 1, d: 1)` | B-04 | Андрущенко Альона | ✅ пройшов | 36 |
| 58 | ProfileTests | `Validate_CorrectProfile_NoErrors` | B-04 | Андрущенко Альона | ✅ пройшов | 20 |
| 59 | ProfileTests | `Validate_EmptyName_Error` | B-04 | Андрущенко Альона | ✅ пройшов | 30 |
| 60 | ProfileTests | `Validate_OnlyLatitude_Error` | B-04 | Андрущенко Альона | ✅ пройшов | 26 |

## Інтеграційні тести (`tests/BloodLink.IntegrationTests`)

Застосунок піднімається в пам'яті через `WebApplicationFactory<Program>` з окремою SQLite-базою; тести шлють справжні HTTP-запити й проходять маршрутизацію, JWT, валідацію і EF Core.

| № | Клас | Тест | Задача | Автор | Результат | Час, мс |
| --- | --- | --- | --- | --- | --- | --- |
| 1 | ApiTests | `BloodRequest_Create_Respond_SeeContact_Fulfill` | B-10, B-28 | Частухіна Юлія | ✅ пройшов | 920 |
| 2 | ApiTests | `BloodRequest_InvalidPhone_Returns400` | B-10, B-28 | Частухіна Юлія | ✅ пройшов | 130 |
| 3 | ApiTests | `BloodRequests_WithoutToken_Return401` | B-10, B-28 | Частухіна Юлія | ✅ пройшов | 17 |
| 4 | ApiTests | `Centers_OnlyLatitude_Returns400` | B-10, B-28 | Частухіна Юлія | ✅ пройшов | 32 |
| 5 | ApiTests | `DemoScenario_Register_Profile_Eligibility_Centers_History` | B-10, B-28 | Частухіна Юлія | ✅ пройшов | 246 |
| 6 | ApiTests | `Donations_AreIsolatedBetweenDonors` | B-10, B-28 | Частухіна Юлія | ✅ пройшов | 434 |
| 7 | ApiTests | `Donations_FutureDate_Returns400` | B-10, B-28 | Частухіна Юлія | ✅ пройшов | 238 |
| 8 | ApiTests | `Login_AfterRegister_Returns200_AndWrongPassword401` | B-10, B-28 | Частухіна Юлія | ✅ пройшов | 3063 |
| 9 | ApiTests | `Profile_Underage_Returns400WithErrors` | B-10, B-28 | Частухіна Юлія | ✅ пройшов | 131 |
| 10 | ApiTests | `ProtectedEndpoint_WithForgedToken_Returns401` | B-10, B-28 | Частухіна Юлія | ✅ пройшов | 34 |
| 11 | ApiTests | `ProtectedEndpoints_WithoutToken_Return401(url: "/api/donations")` | B-10, B-28 | Частухіна Юлія | ✅ пройшов | 2 |
| 12 | ApiTests | `ProtectedEndpoints_WithoutToken_Return401(url: "/api/eligibility")` | B-10, B-28 | Частухіна Юлія | ✅ пройшов | 3 |
| 13 | ApiTests | `ProtectedEndpoints_WithoutToken_Return401(url: "/api/profile")` | B-10, B-28 | Частухіна Юлія | ✅ пройшов | 4 |
| 14 | ApiTests | `Register_DuplicateEmail_Returns409` | B-10, B-28 | Частухіна Юлія | ✅ пройшов | 131 |
| 15 | ApiTests | `Register_ShortPassword_Returns400` | B-10, B-28 | Частухіна Юлія | ✅ пройшов | 14 |
| 16 | ApiTests | `Swagger_DocumentDescribesAllEndpoints` | B-10, B-28 | Частухіна Юлія | ✅ пройшов | 187 |
