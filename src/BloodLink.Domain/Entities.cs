namespace BloodLink.Domain;

public class Donor
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public Sex? Sex { get; set; }
    public DateOnly? BirthDate { get; set; }
    public BloodGroup? BloodGroup { get; set; }
    public RhFactor? RhFactor { get; set; }
    public string? City { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<Donation> Donations { get; set; } = new();
}

public class BloodCenter
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string? Phone { get; set; }
    public string? WorkingHours { get; set; }
}

public class Donation
{
    public int Id { get; set; }
    public int DonorId { get; set; }
    public Donor? Donor { get; set; }
    public int? BloodCenterId { get; set; }
    public BloodCenter? BloodCenter { get; set; }
    public DateOnly Date { get; set; }
    public DonationType Type { get; set; }
    public string? Notes { get; set; }
}
