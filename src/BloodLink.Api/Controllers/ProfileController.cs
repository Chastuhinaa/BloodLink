using System.IdentityModel.Tokens.Jwt;
using BloodLink.Api.Dtos;
using BloodLink.Api.Services;
using BloodLink.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BloodLink.Api.Controllers;

/// <summary>Профіль поточного донора.</summary>
[ApiController]
[Authorize]
[Route("api/profile")]
[Produces("application/json")]
public class ProfileController : DonorControllerBase
{
    private readonly IProfileService _profiles;

    public ProfileController(IProfileService profiles) => _profiles = profiles;

    /// <summary>Отримати свій профіль.</summary>
    [HttpGet]
    [ProducesResponseType<ProfileDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ProfileDto>> Get(CancellationToken ct) =>
        Ok(await _profiles.GetAsync(CurrentDonorId, ct));

    /// <summary>Оновити свій профіль: група, резус, стать, дата народження, місто, координати.</summary>
    /// <response code="400">Вік поза межами 18–65, некоректні координати тощо.</response>
    [HttpPut]
    [ProducesResponseType<ProfileDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ProfileDto>> Update(UpdateProfileRequest request, CancellationToken ct) =>
        Ok(await _profiles.UpdateAsync(CurrentDonorId, request, ct));
}
