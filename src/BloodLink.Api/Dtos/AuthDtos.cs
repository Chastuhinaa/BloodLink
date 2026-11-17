using System.ComponentModel.DataAnnotations;

namespace BloodLink.Api.Dtos;

/// <summary>Дані для реєстрації донора.</summary>
public record RegisterRequest(
    [Required(ErrorMessage = "Вкажіть email."), EmailAddress(ErrorMessage = "Некоректний email."), MaxLength(254)] string Email,
    [Required(ErrorMessage = "Вкажіть пароль."), MinLength(8, ErrorMessage = "Пароль має містити щонайменше 8 символів."), MaxLength(100)] string Password,
    [Required(ErrorMessage = "Вкажіть ім'я."), MaxLength(100)] string FullName);

/// <summary>Дані для входу.</summary>
public record LoginRequest(
    [Required(ErrorMessage = "Вкажіть email."), EmailAddress(ErrorMessage = "Некоректний email.")] string Email,
    [Required(ErrorMessage = "Вкажіть пароль.")] string Password);

/// <summary>Відповідь із JWT-токеном.</summary>
public record AuthResponse(string Token, DateTime ExpiresAt, int DonorId, string FullName);
