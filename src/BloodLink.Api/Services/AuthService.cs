using BloodLink.Api.Data;
using BloodLink.Api.Dtos;
using BloodLink.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BloodLink.Api.Services;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default);
    Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default);
}

public class AuthService : IAuthService
{
    private readonly BloodLinkDbContext _db;
    private readonly ITokenService _tokens;
    private readonly IPasswordHasher<Donor> _hasher;

    public AuthService(BloodLinkDbContext db, ITokenService tokens, IPasswordHasher<Donor> hasher)
    {
        _db = db;
        _tokens = tokens;
        _hasher = hasher;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        var email = NormalizeEmail(request.Email);
        if (await _db.Donors.AnyAsync(d => d.Email == email, ct))
        {
            throw ServiceException.Conflict("Донор з таким email уже зареєстрований.");
        }

        var donor = new Donor
        {
            Email = email,
            FullName = request.FullName.Trim(),
        };
        donor.PasswordHash = _hasher.HashPassword(donor, request.Password);

        _db.Donors.Add(donor);
        await _db.SaveChangesAsync(ct);

        return Issue(donor);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var email = NormalizeEmail(request.Email);
        var donor = await _db.Donors.FirstOrDefaultAsync(d => d.Email == email, ct);

        // Однакова відповідь для «немає користувача» і «невірний пароль», щоб не розкривати наявність email.
        if (donor is null ||
            _hasher.VerifyHashedPassword(donor, donor.PasswordHash, request.Password) == PasswordVerificationResult.Failed)
        {
            throw ServiceException.Unauthorized("Невірний email або пароль.");
        }

        return Issue(donor);
    }

    public static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();

    private AuthResponse Issue(Donor donor)
    {
        var (token, expires) = _tokens.CreateToken(donor);
        return new AuthResponse(token, expires, donor.Id, donor.FullName);
    }
}
