namespace BloodLink.Domain;

public record EligibilityResult(
    bool CanDonateNow,
    DateOnly NextDate,
    int DaysLeft,
    string Reason);

public class DonationIntervalCalculator
{
    private readonly DonationRules _rules;

    public DonationIntervalCalculator(DonationRules? rules = null)
    {
        _rules = rules ?? DonationRules.Default;
    }

    public EligibilityResult Calculate(Sex? sex, IEnumerable<Donation> history, DonationType next, DateOnly today)
    {
        ArgumentNullException.ThrowIfNull(history);

        var donations = history.Where(d => d.Date <= today).OrderByDescending(d => d.Date).ToList();
        if (donations.Count == 0)
        {
            return new EligibilityResult(true, today, 0, "Донацій ще не було — можна здавати.");
        }

        var last = donations[0];
        var interval = _rules.MinIntervalDays(last.Type, next, sex);
        var byInterval = last.Date.AddDays(interval);
        var reason = $"Після останньої донації ({last.Date:dd.MM.yyyy}) потрібно {interval} дн.";

        var nextDate = byInterval;
        if (next == DonationType.WholeBlood)
        {
            var limit = _rules.MaxWholeBloodPerYear(sex);
            var wholeBlood = donations.Where(d => d.Type == DonationType.WholeBlood).ToList();
            if (wholeBlood.Count >= limit)
            {
                // Звільниться місце, коли найстаріша з останніх `limit` донацій вийде за 365 днів.
                var oldestInWindow = wholeBlood[limit - 1].Date;
                var byLimit = oldestInWindow.AddDays(365);
                if (byLimit > nextDate)
                {
                    nextDate = byLimit;
                    reason = $"Досягнуто ліміту {limit} донацій цільної крові за рік.";
                }
            }
        }

        var daysLeft = Math.Max(0, nextDate.DayNumber - today.DayNumber);
        var canDonate = daysLeft == 0;
        if (canDonate)
        {
            reason = "Мінімальний інтервал минув — можна здавати.";
            nextDate = today;
        }

        return new EligibilityResult(canDonate, nextDate, daysLeft, reason);
    }
}

// Значення орієнтовні: звірити з чинним наказом МОЗ України перед пілотом.
public class DonationRules
{
    public int WholeBloodAfterWholeBloodMale { get; init; } = 60;
    public int WholeBloodAfterWholeBloodFemale { get; init; } = 90;
    public int AfterPlasmaOrPlatelets { get; init; } = 14;
    public int ComponentAfterWholeBlood { get; init; } = 30;
    public int MaxWholeBloodPerYearMale { get; init; } = 5;
    public int MaxWholeBloodPerYearFemale { get; init; } = 4;

    public static DonationRules Default { get; } = new();

    public int MinIntervalDays(DonationType previous, DonationType next, Sex? sex)
    {
        if (previous == DonationType.WholeBlood)
        {
            if (next == DonationType.WholeBlood)
            {
                return sex == Sex.Male ? WholeBloodAfterWholeBloodMale : WholeBloodAfterWholeBloodFemale;
            }

            return ComponentAfterWholeBlood;
        }

        return AfterPlasmaOrPlatelets;
    }

    public int MaxWholeBloodPerYear(Sex? sex) =>
        sex == Sex.Male ? MaxWholeBloodPerYearMale : MaxWholeBloodPerYearFemale;
}
