using Microsoft.EntityFrameworkCore;

namespace BloodLink.Api.Data;

public class BloodLinkDbContext : DbContext
{
    public BloodLinkDbContext(DbContextOptions<BloodLinkDbContext> options) : base(options)
    {
    }
}
