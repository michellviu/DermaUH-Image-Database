using System.Security.Claims;
using Application.DermaImage.DTOs;
using Application.DermaImage.Managers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.DermaImage.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthManager _auth;
    private readonly IConfiguration _config;

    public AuthController(IAuthManager auth, IConfiguration config)
    {
        _auth = auth;
        _config = config;
    }

    // ── Register ────────────────────────────────────────────────────────

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto, CancellationToken ct)
    {
        try
        {
            var frontendBase = _config["FrontendBaseUrl"] ?? "http://localhost:5262";
            var confirmationUrl = $"{frontendBase}/account/confirm-email";

            var (success, error) = await _auth.RegisterAsync(dto, confirmationUrl, ct);
            if (!success)
                return BadRequest(new { message = error });

            return Ok(new { message = "Registro exitoso. Revisa tu correo para confirmar la cuenta." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"No fue posible completar el registro: {ex.Message}" });
        }
    }

    // ── Login ───────────────────────────────────────────────────────────

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto, CancellationToken ct)
    {
        var (response, error) = await _auth.LoginAsync(dto, ct);
        if (response is null)
            return Unauthorized(new { message = error });

        return Ok(response);
    }

    // ── Google Login ────────────────────────────────────────────────────

    [AllowAnonymous]
    [HttpPost("google")]
    public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginDto dto, CancellationToken ct)
    {
        var (response, error) = await _auth.GoogleLoginAsync(dto, ct);
        if (response is null)
            return Unauthorized(new { message = error });

        return Ok(response);
    }

    // ── Confirm Email ───────────────────────────────────────────────────

    [AllowAnonymous]
    [HttpPost("confirm-email")]
    public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmEmailDto dto, CancellationToken ct)
    {
        var (success, error) = await _auth.ConfirmEmailAsync(dto, ct);
        if (!success)
            return BadRequest(new { message = error });

        return Ok(new { message = "Correo confirmado correctamente. Ya puedes iniciar sesión." });
    }

    // ── Forgot Password ─────────────────────────────────────────────────

    [AllowAnonymous]
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto, CancellationToken ct)
    {
        var frontendBase = _config["FrontendBaseUrl"] ?? "http://localhost:5262";
        var resetUrl = $"{frontendBase}/account/reset-password";

        await _auth.ForgotPasswordAsync(dto, resetUrl, ct);
        return Ok(new { message = "Si la cuenta existe, recibirás un correo con instrucciones." });
    }

    // ── Reset Password ──────────────────────────────────────────────────

    [AllowAnonymous]
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto, CancellationToken ct)
    {
        var (success, error) = await _auth.ResetPasswordAsync(dto, ct);
        if (!success)
            return BadRequest(new { message = error });

        return Ok(new { message = "Contraseña restablecida correctamente." });
    }

    // ── Profile (authenticated) ─────────────────────────────────────────

    [Authorize]
    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile(CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        if (userId is null) return Unauthorized();

        var profile = await _auth.GetProfileAsync(userId.Value, ct);
        if (profile is null) return NotFound();

        return Ok(profile);
    }

    [Authorize]
    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        if (userId is null) return Unauthorized();

        var (success, error) = await _auth.UpdateProfileAsync(userId.Value, dto, ct);
        if (!success) return BadRequest(new { message = error });

        return Ok(new { message = "Perfil actualizado." });
    }

    [Authorize]
    [HttpPost("profile/confirm-phone")]
    public async Task<IActionResult> ConfirmPhone([FromBody] ConfirmPhoneDto dto, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        if (userId is null) return Unauthorized();

        var (success, error) = await _auth.ConfirmPhoneAsync(userId.Value, dto, ct);
        if (!success) return BadRequest(new { message = error });

        return Ok(new { message = "Teléfono móvil confirmado correctamente." });
    }

    [Authorize]
    [HttpPut("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        if (userId is null) return Unauthorized();

        try
        {
            var (success, error) = await _auth.ChangePasswordAsync(userId.Value, dto, ct);
            if (!success) return BadRequest(new { message = error });

            return Ok(new { message = "Contraseña actualizada." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"No fue posible cambiar la contraseña: {ex.Message}" });
        }
    }

    [Authorize]
    [HttpPost("institution-membership-requests")]
    public async Task<IActionResult> CreateInstitutionMembershipRequest([FromBody] CreateInstitutionMembershipRequestDto dto, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        if (userId is null) return Unauthorized();

        var (request, error) = await _auth.CreateInstitutionMembershipRequestAsync(userId.Value, dto, ct);
        if (request is null) return BadRequest(new { message = error });

        return Ok(request);
    }

    [Authorize]
    [HttpGet("institution-membership-requests/mine")]
    public async Task<IActionResult> GetMyInstitutionMembershipRequests(CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        if (userId is null) return Unauthorized();

        var requests = await _auth.GetMyInstitutionMembershipRequestsAsync(userId.Value, ct);
        return Ok(requests);
    }

    [Authorize]
    [HttpGet("institution-membership-requests/inbox")]
    public async Task<IActionResult> GetInstitutionInbox(CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        if (userId is null) return Unauthorized();

        var (requests, error) = await _auth.GetInstitutionInboxAsync(userId.Value, ct);
        if (requests is null) return Forbid();

        return Ok(requests);
    }

    [Authorize]
    [HttpPost("institution-membership-requests/{requestId:guid}/review")]
    public async Task<IActionResult> ReviewInstitutionMembershipRequest(Guid requestId, [FromBody] ReviewInstitutionMembershipRequestDto dto, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        if (userId is null) return Unauthorized();

        var (request, error) = await _auth.ReviewInstitutionMembershipRequestAsync(userId.Value, requestId, dto, ct);
        if (request is null)
        {
            if (!string.IsNullOrWhiteSpace(error) && error.Contains("permisos", StringComparison.OrdinalIgnoreCase))
                return Forbid();

            return BadRequest(new { message = error });
        }

        return Ok(request);
    }

    // ── Helpers ─────────────────────────────────────────────────────────

    private Guid? GetCurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(value, out var id) ? id : null;
    }
}
