# BloodLink

**Кров приходить раніше, ніж закінчиться.** Застосунок, що з'єднує донорів із центрами крові й з людьми, яким потрібна кров. Донору він підказує, коли знову можна здавати і де найближчий центр. Тому, хто шукає кров для себе чи близької людини, — допомагає швидко знайти сумісних донорів поруч. Навесні додамо SOS-сповіщення донорам потрібної групи поруч і ML-прогноз дефіциту.

Командний проєкт, ТТП-42: Андрущенко Альона, Дорошенко Дарія, Лисенко Катерина, Частухіна Юлія.

## Що вміє версія модуля 2

- Реєстрація і вхід (JWT).
- Профіль донора: група крові, резус, стать, дата народження, місце.
- Калькулятор: коли можна здавати наступного разу (інтервали й річний ліміт).
- Центри крові, відсортовані за відстанню.
- Історія донацій: додати, переглянути, видалити.
- Запити на кров: людина створює запит для пацієнта, донори з сумісною групою поруч відгукуються «Я допоможу» і отримують телефон для зв'язку.
- Мінімальний клієнт (вхід, профіль, центри, запити, історія) і Swagger-документація API.

## Запуск

Потрібен [.NET 9 SDK](https://dotnet.microsoft.com/download).

```bash
dotnet run --project src/BloodLink.Api
```

- Клієнт: <http://localhost:5124>
- Swagger: <http://localhost:5124/swagger>

За замовчуванням використовується SQLite (файл `src/BloodLink.Api/bloodlink.db` створюється сам).

### З PostgreSQL

```bash
docker compose up -d
dotnet run --project src/BloodLink.Api --launch-profile postgres
```

## Тести

```bash
dotnet test
```

Журнал результатів: [docs/test-log.md](docs/test-log.md). Перегенерувати:

```bash
dotnet test --logger "trx;LogFileName=unit.trx" --results-directory TestResults tests/BloodLink.UnitTests
dotnet test --logger "trx;LogFileName=integration.trx" --results-directory TestResults tests/BloodLink.IntegrationTests
python tools/trx_to_md.py TestResults/unit.trx TestResults/integration.trx -o docs/test-log.md
```

## Документація

| Документ | Зміст |
| --- | --- |
| [docs/architecture.md](docs/architecture.md) | Архітектура системи й обґрунтування інструментів |
| [docs/uml.md](docs/uml.md) | Діаграми варіантів використання, класів, послідовності |
| [docs/database.md](docs/database.md) | ER-діаграма і схема БД |
| [docs/api.md](docs/api.md) | Ендпоінти, приклади, коди помилок; [openapi.json](docs/openapi.json) |
| [docs/test-log.md](docs/test-log.md) | Журнал модульних та інтеграційних тестів |
| [docs/sprint.md](docs/sprint.md) | План спринту, критерії готовності, сценарій демо, рефлексія |
| [docs/git-workflow.md](docs/git-workflow.md) | Гілки, Pull Request, code review |

## Стек

ASP.NET Core 9 · Entity Framework Core 9 · PostgreSQL + PostGIS (SQLite локально) · JWT · Swagger · xUnit · GitHub Actions.
