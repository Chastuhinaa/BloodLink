using BloodLink.Api.Dtos;
using BloodLink.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BloodLink.Api.Controllers;

/// <summary>Запити на кров від людей, яким вона потрібна, і відгуки донорів.</summary>
[ApiController]
[Authorize]
[Route("api/requests")]
[Produces("application/json")]
public class BloodRequestsController : DonorControllerBase
{
    private readonly IBloodRequestService _requests;

    public BloodRequestsController(IBloodRequestService requests) => _requests = requests;

    /// <summary>Відкриті запити: спершу найтерміновіші, потім найближчі.</summary>
    /// <param name="lat">Широта. Без неї береться точка з профілю.</param>
    /// <param name="lng">Довгота.</param>
    /// <param name="radiusKm">Показати лише запити в цьому радіусі.</param>
    /// <param name="onlyCompatible">Лише ті, де поточний донор може допомогти.</param>
    /// <param name="ct">Токен скасування.</param>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<BloodRequestDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<BloodRequestDto>>> List(
        [FromQuery] double? lat, [FromQuery] double? lng, [FromQuery] double? radiusKm,
        [FromQuery] bool onlyCompatible = false, CancellationToken ct = default) =>
        Ok(await _requests.ListOpenAsync(CurrentDonorId, lat, lng, radiusKm, onlyCompatible, ct));

    /// <summary>Мої запити разом зі списком донорів, які відгукнулися.</summary>
    [HttpGet("mine")]
    [ProducesResponseType<IReadOnlyList<BloodRequestDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<BloodRequestDto>>> Mine(CancellationToken ct) =>
        Ok(await _requests.ListMineAsync(CurrentDonorId, ct));

    /// <summary>Один запит.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType<BloodRequestDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<BloodRequestDto>> Get(int id, CancellationToken ct) =>
        Ok(await _requests.GetAsync(CurrentDonorId, id, ct));

    /// <summary>Створити запит на кров для пацієнта.</summary>
    /// <response code="201">Запит створено.</response>
    /// <response code="400">Некоректний телефон, невідомий центр, дата в минулому тощо.</response>
    [HttpPost]
    [ProducesResponseType<BloodRequestDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<BloodRequestDto>> Create(CreateBloodRequestRequest request, CancellationToken ct)
    {
        var created = await _requests.CreateAsync(CurrentDonorId, request, ct);
        return StatusCode(StatusCodes.Status201Created, created);
    }

    /// <summary>«Я допоможу»: відгукнутися на запит. Після цього донору видно телефон автора.</summary>
    /// <response code="200">Відгук збережено.</response>
    /// <response code="400">Несумісна група, ще не минув інтервал, власний або закритий запит.</response>
    /// <response code="409">Уже відгукнулися.</response>
    [HttpPost("{id:int}/responses")]
    [ProducesResponseType<BloodRequestDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<BloodRequestDto>> Respond(int id, CancellationToken ct) =>
        Ok(await _requests.RespondAsync(CurrentDonorId, id, ct));

    /// <summary>Скасувати свій відгук.</summary>
    [HttpDelete("{id:int}/responses")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CancelResponse(int id, CancellationToken ct)
    {
        await _requests.CancelResponseAsync(CurrentDonorId, id, ct);
        return NoContent();
    }

    /// <summary>Позначити свій запит як виконаний або скасований.</summary>
    /// <response code="404">Запиту немає або він не ваш.</response>
    [HttpPatch("{id:int}/status")]
    [ProducesResponseType<BloodRequestDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<BloodRequestDto>> UpdateStatus(int id, UpdateRequestStatusRequest request, CancellationToken ct) =>
        Ok(await _requests.UpdateStatusAsync(CurrentDonorId, id, request.Status, ct));
}
