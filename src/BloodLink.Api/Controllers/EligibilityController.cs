using System.IdentityModel.Tokens.Jwt;
using BloodLink.Api.Dtos;
using BloodLink.Api.Services;
using BloodLink.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BloodLink.Api.Controllers;

/// <summary>Калькулятор наступної здачі.</summary>
[ApiController]
[Authorize]
[Route("api/eligibility")]
[Produces("application/json")]
public class EligibilityController : DonorControllerBase
{
    private readonly IEligibilityService _eligibility;

    public EligibilityController(IEligibilityService eligibility) => _eligibility = eligibility;

    /// <summary>Коли поточний донор може здати кров (або компонент) наступного разу.</summary>
    /// <param name="type">Тип запланованої донації: WholeBlood, Plasma або Platelets.</param>
    /// <param name="ct">Токен скасування.</param>
    [HttpGet]
    [ProducesResponseType<EligibilityDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<EligibilityDto>> Get([FromQuery] DonationType type = DonationType.WholeBlood, CancellationToken ct = default) =>
        Ok(await _eligibility.GetAsync(CurrentDonorId, type, ct));
}
