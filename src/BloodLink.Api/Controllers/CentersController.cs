using System.IdentityModel.Tokens.Jwt;
using BloodLink.Api.Dtos;
using BloodLink.Api.Services;
using BloodLink.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BloodLink.Api.Controllers;

/// <summary>Центри крові.</summary>
[ApiController]
[Route("api/centers")]
[Produces("application/json")]
public class CentersController : ControllerBase
{
    private readonly ICenterService _centers;

    public CentersController(ICenterService centers) => _centers = centers;

    /// <summary>Список центрів крові, від найближчого до найдальшого.</summary>
    /// <param name="lat">Широта донора. Без координат список сортується за назвою.</param>
    /// <param name="lng">Довгота донора.</param>
    /// <param name="limit">Скільки центрів повернути.</param>
    /// <param name="ct">Токен скасування.</param>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<CenterDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<CenterDto>>> Get(
        [FromQuery] double? lat, [FromQuery] double? lng, [FromQuery] int? limit, CancellationToken ct) =>
        Ok(await _centers.GetAsync(lat, lng, limit, ct));
}
