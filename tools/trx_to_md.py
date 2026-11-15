"""Перетворює результати тестів (.trx) на журнал тестування в Markdown.

Використання:
    dotnet test --logger "trx;LogFileName=unit.trx" --results-directory TestResults
    python tools/trx_to_md.py TestResults/unit.trx TestResults/integration.trx -o docs/test-log.md
"""
import argparse
import datetime as dt
import xml.etree.ElementTree as ET

NS = {"t": "http://microsoft.com/schemas/VisualStudio/TeamTest/2010"}

# Хто відповідає за тестовий клас (за розподілом задач модуля 2).
OWNERS = {
    "DonationIntervalCalculatorTests": ("B-05", "Лисенко Катерина"),
    "GeoAndCentersTests": ("B-06", "Дорошенко Дарія"),
    "DonationServiceTests": ("B-07", "Дорошенко Дарія"),
    "ProfileTests": ("B-04", "Андрущенко Альона"),
    "AuthTests": ("B-03", "Частухіна Юлія"),
    "ApiTests": ("B-10, B-28", "Частухіна Юлія"),
    "BloodCompatibilityTests": ("B-28", "Частухіна Юлія"),
    "BloodRequestServiceTests": ("B-28", "Частухіна Юлія"),
}

OUTCOME = {"Passed": "✅ пройшов", "Failed": "❌ не пройшов", "NotExecuted": "⏭ пропущено"}


def parse(path):
    root = ET.parse(path).getroot()
    full_classes = {
        d.get("id"): d.find("t:TestMethod", NS).get("className")
        for d in root.iterfind(".//t:TestDefinitions/t:UnitTest", NS)
    }
    rows = []
    for r in root.iterfind(".//t:Results/t:UnitTestResult", NS):
        full = full_classes.get(r.get("testId"), "?")
        cls = full.split(".")[-1]
        name = r.get("testName")
        if name.startswith(full + "."):
            name = name[len(full) + 1:]
        duration = r.get("duration", "00:00:00")
        h, m, s = duration.split(":")
        ms = (int(h) * 3600 + int(m) * 60 + float(s)) * 1000
        rows.append((cls, name, r.get("outcome"), ms))
    times = root.find("t:Times", NS)
    return rows, times.get("finish") if times is not None else ""


def table(rows):
    out = ["| № | Клас | Тест | Задача | Автор | Результат | Час, мс |", "| --- | --- | --- | --- | --- | --- | --- |"]
    for i, (cls, name, outcome, ms) in enumerate(sorted(rows), 1):
        task, owner = OWNERS.get(cls, ("—", "—"))
        out.append(f"| {i} | {cls} | `{name}` | {task} | {owner} | {OUTCOME.get(outcome, outcome)} | {ms:.0f} |")
    return "\n".join(out)


def summary(rows):
    passed = sum(1 for r in rows if r[2] == "Passed")
    failed = sum(1 for r in rows if r[2] == "Failed")
    return passed, failed, len(rows)


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("unit")
    ap.add_argument("integration")
    ap.add_argument("-o", "--output", required=True)
    args = ap.parse_args()

    unit, finished = parse(args.unit)
    integ, _ = parse(args.integration)
    up, uf, ut = summary(unit)
    ip, if_, it = summary(integ)

    by_owner = {}
    for cls, *_ in unit:
        owner = OWNERS.get(cls, ("—", "—"))[1]
        by_owner[owner] = by_owner.get(owner, 0) + 1

    when = finished[:19].replace("T", " ") if finished else dt.datetime.now().strftime("%Y-%m-%d %H:%M:%S")
    lines = [
        "# Журнал результатів тестування (B-09, B-10)",
        "",
        f"**Підсумок:** модульні — {up} з {ut} пройшли, {uf} не пройшли; "
        f"інтеграційні — {ip} з {it} пройшли, {if_} не пройшли. Прогін: {when} (UTC).",
        "",
        "Журнал згенеровано з `.trx`-файлів командою `python tools/trx_to_md.py` — не редагуйте його вручну, "
        "а перегенеруйте після змін. Кожен випадок `[Theory]` з `InlineData` рахується окремим тестом.",
        "",
        "## Модульні тести за учасницями",
        "",
        "| Учасниця | Кількість тестів |",
        "| --- | --- |",
        *[f"| {o} | {n} |" for o, n in sorted(by_owner.items())],
        "",
        "## Модульні тести (`tests/BloodLink.UnitTests`)",
        "",
        table(unit),
        "",
        "## Інтеграційні тести (`tests/BloodLink.IntegrationTests`)",
        "",
        "Застосунок піднімається в пам'яті через `WebApplicationFactory<Program>` з окремою SQLite-базою; "
        "тести шлють справжні HTTP-запити й проходять маршрутизацію, JWT, валідацію і EF Core.",
        "",
        table(integ),
        "",
    ]
    with open(args.output, "w", encoding="utf-8") as f:
        f.write("\n".join(lines))
    print(f"unit {up}/{ut}, integration {ip}/{it} -> {args.output}")


if __name__ == "__main__":
    main()
