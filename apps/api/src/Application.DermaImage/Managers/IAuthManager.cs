using Application.DermaImage.DTOs;

namespace Application.DermaImage.Managers;

public interface IAuthManager
{
    Task<(bool Success, string? Error)> RegisterAsync(RegisterDto dto, string confirmationBaseUrl, CancellationToken ct = default);
    Task<(LoginResponseDto? Response, string? Error)> LoginAsync(LoginDto dto, CancellationToken ct = default);
    Task<(LoginResponseDto? Response, string? Error)> GoogleLoginAsync(GoogleLoginDto dto, CancellationToken ct = default);
    Task<(bool Success, string? Error)> ConfirmEmailAsync(ConfirmEmailDto dto, CancellationToken ct = default);
    Task<(bool Success, string? Error)> ForgotPasswordAsync(ForgotPasswordDto dto, string resetBaseUrl, CancellationToken ct = default);
    Task<(bool Success, string? Error)> ResetPasswordAsync(ResetPasswordDto dto, CancellationToken ct = default);
    Task<UserProfileDto?> GetProfileAsync(Guid userId, CancellationToken ct = default);
    Task<(bool Success, string? Error)> UpdateProfileAsync(Guid userId, UpdateProfileDto dto, CancellationToken ct = default);
    Task<(bool Success, string? Error)> ConfirmPhoneAsync(Guid userId, ConfirmPhoneDto dto, CancellationToken ct = default);
    Task<(bool Success, string? Error)> ChangePasswordAsync(Guid userId, ChangePasswordDto dto, CancellationToken ct = default);
    Task<(InstitutionMembershipRequestDto? Request, string? Error)> CreateInstitutionMembershipRequestAsync(Guid userId, CreateInstitutionMembershipRequestDto dto, CancellationToken ct = default);
    Task<IEnumerable<InstitutionMembershipRequestDto>> GetMyInstitutionMembershipRequestsAsync(Guid userId, CancellationToken ct = default);
    Task<(IEnumerable<InstitutionMembershipRequestDto>? Requests, string? Error)> GetInstitutionInboxAsync(Guid responsibleUserId, CancellationToken ct = default);
    Task<(InstitutionMembershipRequestDto? Request, string? Error)> ReviewInstitutionMembershipRequestAsync(Guid responsibleUserId, Guid requestId, ReviewInstitutionMembershipRequestDto dto, CancellationToken ct = default);
}
