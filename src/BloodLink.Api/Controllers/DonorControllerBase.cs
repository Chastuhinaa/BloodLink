using System.IdentityModel.Tokens.Jwt;
using BloodLink.Api.Dtos;
using BloodLink.Api.Services;
using BloodLink.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BloodLink.Api.Controllers;

/// <summary>Базовий контролер: id поточного донора з JWT.</summary>
public abstract class DonorControllerBase : ControllerBase
{
    protected int CurrentDonorId =>
        int.TryParse(User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value, out var id)
            ? id
            : throw ServiceException.Unauthorized("Недійсний токен.");
}
