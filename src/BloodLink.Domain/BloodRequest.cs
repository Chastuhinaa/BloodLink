namespace BloodLink.Domain;

public enum RequestUrgency
{
    Normal = 1,
    Urgent = 2,
    Critical = 3
}

public enum RequestStatus
{
    Open = 1,
    Fulfilled = 2,
    Cancelled = 3
}

public class BloodRequest
{
    public int Id { get; set; }
    public int AuthorId { get; set; }
    public Donor? Author { get; set; }
    public BloodGroup BloodGroup { get; set; }
    public RhFactor RhFactor { get; set; }
    public DonationType Type { get; set; } = DonationType.WholeBlood;
    public int UnitsNeeded { get; set; } = 1;
    public int BloodCenterId { get; set; }
    public BloodCenter? BloodCenter { get; set; }
    public RequestUrgency Urgency { get; set; } = RequestUrgency.Normal;
    public RequestStatus Status { get; set; } = RequestStatus.Open;
    public string ContactPhone { get; set; } = string.Empty;
    public string? PatientName { get; set; }
    public string? Hospital { get; set; }
    public string? Description { get; set; }
    public DateOnly? NeededBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<RequestResponse> Responses { get; set; } = new();
}

public class RequestResponse
{
    public int Id { get; set; }
    public int RequestId { get; set; }
    public BloodRequest? Request { get; set; }
    public int DonorId { get; set; }
    public Donor? Donor { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
