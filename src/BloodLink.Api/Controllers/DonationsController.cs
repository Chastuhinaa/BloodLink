using System.IdentityModel.Tokens.Jwt;
using BloodLink.Api.Dtos;
using BloodLink.Api.Services;
using BloodLink.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BloodLink.Api.Controllers;

/// <summary>Історія донацій поточного донора.</summary>
[ApiController]
[Authorize]
[Route("api/donations")]
[Produces("application/json")]
public class DonationsController : DonorControllerBase
{
    private readonly IDonationService _donations;

    public DonationsController(IDonationService donations) => _donations = donations;

    /// <summary>Уся історія донацій, від нових до старих.</summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<DonationDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<DonationDto>>> List(CancellationToken ct) =>
        Ok(await _donations.ListAsync(CurrentDonorId, ct));

    /// <summary>Додати донацію в історію.</summary>
    /// <response code="201">Запис створено.</response>
    /// <response code="400">Дата в майбутньому, невідомий центр тощо.</response>
    [HttpPost]
    [ProducesResponseType<DonationDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<DonationDto>> Add(CreateDonationRequest request, CancellationToken ct)
    {
        var created = await _donations.AddAsync(CurrentDonorId, request, ct);
        return StatusCode(StatusCodes.Status201Created, created);
    }

    /// <summary>Видалити помилковий запис.</summary>
    /// <response code="204">Видалено.</response>
    /// <response code="404">Запису немає або він належить іншому донору.</response>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await _donations.DeleteAsync(CurrentDonorId, id, ct);
        return NoContent();
    }
}
