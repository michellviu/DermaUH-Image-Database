using System.Net.Http.Json;
using Web.DermaImage.Shared.Models;

namespace Web.DermaImage.Components.Pages.Account;

public partial class Profile
{
    private const int MembershipPageSize = 5;
    private const int ImageReviewPageSize = 5;

    private UserProfile? _profile;
    private UpdateProfileRequest _profileForm = new();
    private ChangePasswordRequest _pwdForm = new();

    private bool _savingProfile;
    private bool _savingPwd;
    private bool _pwdMismatch;
    private bool _sendingJoinRequest;
    private bool _leavingInstitution;
    private bool _loadingMyJoinRequests;
    private bool _loadingInboxRequests;
    private bool _loadingMyImageReviewRequests;
    private bool _loadingReviewImageInbox;

    private string? _profileSuccess;
    private string? _profileError;
    private string? _pwdSuccess;
    private string? _pwdError;
    private string? _membershipMessage;
    private string? _membershipError;
    private Guid? _selectedInstitutionId;

    private List<InstitutionDto> _institutions = [];
    private PagedResponse<InstitutionJoinRequestDto> _myJoinRequestsPage = new()
    {
        Items = [],
        Page = 1,
        PageSize = MembershipPageSize,
    };

    private PagedResponse<InstitutionJoinRequestDto> _inboxRequestsPage = new()
    {
        Items = [],
        Page = 1,
        PageSize = MembershipPageSize,
    };

    private PagedResponse<DermaImgDto> _myImageReviewRequestsPage = new()
    {
        Items = [],
        Page = 1,
        PageSize = ImageReviewPageSize,
    };

    private PagedResponse<DermaImgDto> _reviewImageInboxPage = new()
    {
        Items = [],
        Page = 1,
        PageSize = ImageReviewPageSize,
    };

    private int _myJoinRequestsPageNumber = 1;
    private int _inboxRequestsPageNumber = 1;
    private int _myImageReviewRequestsPageNumber = 1;
    private int _reviewImageInboxPageNumber = 1;
    private readonly Dictionary<Guid, string> _reviewComments = new();
    private readonly Dictionary<Guid, string> _imageReviewComments = new();

    private bool _isReviewerOrAdmin =>
        _profile?.Roles.Any(r =>
            string.Equals(r, "Reviewer", StringComparison.OrdinalIgnoreCase)
            || string.Equals(r, "Admin", StringComparison.OrdinalIgnoreCase)) == true;

    private bool _canRequestInstitutionAssociation =>
        _profile is not null
        && _profile.EmailConfirmed;

    protected override async Task OnInitializedAsync()
    {
        await ReloadProfileAsync();
        await LoadInstitutionDataAsync();
        await LoadMembershipDataAsync();
        await LoadImageReviewDataAsync();
    }

    private async Task SaveProfileAsync()
    {
        _savingProfile = true;
        _profileSuccess = null;
        _profileError = null;

        var (success, error) = await Auth.UpdateProfileAsync(_profileForm);

        _savingProfile = false;

        if (!success)
        {
            _profileError = error;
            return;
        }

        _profileSuccess = "Perfil actualizado correctamente.";
        await ReloadProfileAsync();
        await LoadMembershipDataAsync();
    }

    private async Task ChangePasswordAsync()
    {
        _pwdMismatch = false;
        _pwdSuccess = null;
        _pwdError = null;

        if (_pwdForm.NewPassword != _pwdForm.ConfirmPassword)
        {
            _pwdMismatch = true;
            return;
        }

        _savingPwd = true;
        var (success, error) = await Auth.ChangePasswordAsync(_pwdForm);
        _savingPwd = false;

        if (!success)
        {
            _pwdError = error;
            return;
        }

        _pwdSuccess = "Contraseña actualizada correctamente.";
        _pwdForm = new ChangePasswordRequest();
    }

    private async Task ReloadProfileAsync()
    {
        _profile = await Auth.FetchProfileAsync();

        if (_profile is null)
        {
            return;
        }

        _profileForm = new UpdateProfileRequest
        {
            FirstName = _profile.FirstName,
            LastName = _profile.LastName,
            PhoneNumber = _profile.PhoneNumber,
            InstitutionId = _profile.InstitutionId,
        };
    }

    private async Task LoadInstitutionDataAsync()
    {
        try
        {
            var response = await Http.GetFromJsonAsync<PagedResponse<InstitutionDto>>("api/institutions?page=1&pageSize=200");
            _institutions = response?.Items.ToList() ?? [];
        }
        catch
        {
            _institutions = [];
        }
    }

    private async Task LoadMembershipDataAsync()
    {
        await LoadMyJoinRequestsPageAsync(_myJoinRequestsPageNumber);
        await LoadInboxPageAsync(_inboxRequestsPageNumber);
    }

    private async Task LoadImageReviewDataAsync()
    {
        await LoadMyImageReviewRequestsPageAsync(_myImageReviewRequestsPageNumber);

        if (_isReviewerOrAdmin)
        {
            await LoadReviewImageInboxPageAsync(_reviewImageInboxPageNumber);
        }
        else
        {
            _reviewImageInboxPage = new PagedResponse<DermaImgDto>
            {
                Items = [],
                Page = 1,
                PageSize = ImageReviewPageSize,
            };
        }
    }

    private async Task GoToMyJoinRequestsPageAsync(int page)
    {
        await LoadMyJoinRequestsPageAsync(page);
    }

    private async Task GoToInboxPageAsync(int page)
    {
        await LoadInboxPageAsync(page);
    }

    private async Task GoToMyImageReviewRequestsPageAsync(int page)
    {
        await LoadMyImageReviewRequestsPageAsync(page);
    }

    private async Task GoToReviewImageInboxPageAsync(int page)
    {
        await LoadReviewImageInboxPageAsync(page);
    }

    private async Task LoadMyJoinRequestsPageAsync(int page)
    {
        _loadingMyJoinRequests = true;
        try
        {
            var requestedPage = Math.Max(1, page);
            var response = await Auth.GetMyInstitutionJoinRequestsAsync(requestedPage, MembershipPageSize);
            var normalized = NormalizePagedResponse(response, requestedPage, MembershipPageSize);

            if (normalized.TotalPages > 0 && requestedPage > normalized.TotalPages)
            {
                response = await Auth.GetMyInstitutionJoinRequestsAsync(normalized.TotalPages, MembershipPageSize);
                normalized = NormalizePagedResponse(response, normalized.TotalPages, MembershipPageSize);
            }

            _myJoinRequestsPage = normalized;
            _myJoinRequestsPageNumber = normalized.Page;
        }
        finally
        {
            _loadingMyJoinRequests = false;
        }
    }

    private async Task LoadInboxPageAsync(int page)
    {
        _loadingInboxRequests = true;
        try
        {
            var requestedPage = Math.Max(1, page);
            var response = await Auth.GetResponsibleInboxAsync(requestedPage, MembershipPageSize);
            var normalized = NormalizePagedResponse(response, requestedPage, MembershipPageSize);

            if (normalized.TotalPages > 0 && requestedPage > normalized.TotalPages)
            {
                response = await Auth.GetResponsibleInboxAsync(normalized.TotalPages, MembershipPageSize);
                normalized = NormalizePagedResponse(response, normalized.TotalPages, MembershipPageSize);
            }

            _inboxRequestsPage = normalized;
            _inboxRequestsPageNumber = normalized.Page;
        }
        finally
        {
            _loadingInboxRequests = false;
        }
    }

    private async Task LoadMyImageReviewRequestsPageAsync(int page)
    {
        _loadingMyImageReviewRequests = true;
        try
        {
            var requestedPage = Math.Max(1, page);
            var response = await Auth.GetMyImageReviewRequestsAsync(requestedPage, ImageReviewPageSize);
            var normalized = NormalizePagedResponse(response, requestedPage, ImageReviewPageSize);

            if (normalized.TotalPages > 0 && requestedPage > normalized.TotalPages)
            {
                response = await Auth.GetMyImageReviewRequestsAsync(normalized.TotalPages, ImageReviewPageSize);
                normalized = NormalizePagedResponse(response, normalized.TotalPages, ImageReviewPageSize);
            }

            _myImageReviewRequestsPage = normalized;
            _myImageReviewRequestsPageNumber = normalized.Page;
        }
        finally
        {
            _loadingMyImageReviewRequests = false;
        }
    }

    private async Task LoadReviewImageInboxPageAsync(int page)
    {
        _loadingReviewImageInbox = true;
        try
        {
            var requestedPage = Math.Max(1, page);
            var response = await Auth.GetImageReviewInboxAsync(requestedPage, ImageReviewPageSize);
            var normalized = NormalizePagedResponse(response, requestedPage, ImageReviewPageSize);

            if (normalized.TotalPages > 0 && requestedPage > normalized.TotalPages)
            {
                response = await Auth.GetImageReviewInboxAsync(normalized.TotalPages, ImageReviewPageSize);
                normalized = NormalizePagedResponse(response, normalized.TotalPages, ImageReviewPageSize);
            }

            _reviewImageInboxPage = normalized;
            _reviewImageInboxPageNumber = normalized.Page;
        }
        finally
        {
            _loadingReviewImageInbox = false;
        }
    }

    private async Task SubmitJoinRequestAsync()
    {
        _membershipError = null;
        _membershipMessage = null;

        if (_selectedInstitutionId is null || _selectedInstitutionId == Guid.Empty)
        {
            _membershipError = "Seleccione una institución para solicitar asociación.";
            return;
        }

        _sendingJoinRequest = true;
        var (success, error) = await Auth.CreateInstitutionJoinRequestAsync(_selectedInstitutionId.Value);
        _sendingJoinRequest = false;

        if (!success)
        {
            _membershipError = error;
            return;
        }

        _membershipMessage = "Solicitud enviada correctamente.";
        _myJoinRequestsPageNumber = 1;
        await LoadMembershipDataAsync();
    }

    private async Task LeaveInstitutionAsync()
    {
        _membershipError = null;
        _membershipMessage = null;

        _leavingInstitution = true;
        var (success, error) = await Auth.LeaveMyInstitutionAsync();
        _leavingInstitution = false;

        if (!success)
        {
            _membershipError = error;
            return;
        }

        _membershipMessage = "Has salido de la institución correctamente.";
        _selectedInstitutionId = null;
        await ReloadProfileAsync();
        await LoadMembershipDataAsync();
        await LoadImageReviewDataAsync();
    }

    private async Task ReviewRequestAsync(Guid requestId, bool approve)
    {
        _membershipError = null;
        _membershipMessage = null;

        var comment = _reviewComments.TryGetValue(requestId, out var value)
            ? value
            : null;

        var (success, error) = await Auth.ReviewInstitutionJoinRequestAsync(requestId, new ReviewInstitutionJoinRequest
        {
            Approve = approve,
            Comment = comment,
        });

        if (!success)
        {
            _membershipError = error;
            return;
        }

        _membershipMessage = approve
            ? "Solicitud aprobada."
            : "Solicitud denegada.";

        _reviewComments.Remove(requestId);
        await LoadMembershipDataAsync();
    }

    private async Task ReviewImageAsync(Guid imageId, bool approve)
    {
        _membershipError = null;
        _membershipMessage = null;

        var comment = _imageReviewComments.TryGetValue(imageId, out var value)
            ? value
            : null;

        var (success, error) = await Auth.ReviewImageUploadAsync(imageId, new ReviewImageUploadRequest
        {
            Approve = approve,
            Comment = comment,
        });

        if (!success)
        {
            _membershipError = error;
            return;
        }

        _membershipMessage = approve
            ? "Imagen aprobada correctamente."
            : "Imagen declinada correctamente.";

        _imageReviewComments.Remove(imageId);
        await LoadImageReviewDataAsync();
    }

    private static PagedResponse<T> NormalizePagedResponse<T>(PagedResponse<T>? response, int requestedPage, int fallbackPageSize)
    {
        var source = response ?? new PagedResponse<T>();
        var items = source.Items?.ToList() ?? [];
        var pageSize = source.PageSize > 0 ? source.PageSize : fallbackPageSize;
        var totalPages = source.TotalCount == 0
            ? 0
            : (int)Math.Ceiling(source.TotalCount / (double)pageSize);
        var page = totalPages == 0
            ? 1
            : Math.Clamp(source.Page > 0 ? source.Page : requestedPage, 1, totalPages);

        return new PagedResponse<T>
        {
            Items = items,
            TotalCount = source.TotalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = totalPages,
            HasPrevious = page > 1,
            HasNext = totalPages > 0 && page < totalPages,
        };
    }

    private string GetReviewComment(Guid requestId)
    {
        return _reviewComments.TryGetValue(requestId, out var comment)
            ? comment
            : string.Empty;
    }

    private void SetReviewComment(Guid requestId, string value)
    {
        _reviewComments[requestId] = value;
    }

    private static string MapStatus(string status)
    {
        if (string.Equals(status, "Pending", StringComparison.OrdinalIgnoreCase))
        {
            return "Pendiente";
        }

        if (string.Equals(status, "Approved", StringComparison.OrdinalIgnoreCase))
        {
            return "Aprobada";
        }

        if (string.Equals(status, "Denied", StringComparison.OrdinalIgnoreCase))
        {
            return "Denegada";
        }

        return status;
    }

    private static string MapImageReviewStatus(string status)
    {
        if (string.Equals(status, "Pending", StringComparison.OrdinalIgnoreCase))
        {
            return "Pendiente";
        }

        if (string.Equals(status, "Approved", StringComparison.OrdinalIgnoreCase))
        {
            return "Aprobada";
        }

        if (string.Equals(status, "Declined", StringComparison.OrdinalIgnoreCase))
        {
            return "Declinada";
        }

        return status;
    }

    private string GetImageReviewComment(Guid imageId)
    {
        return _imageReviewComments.TryGetValue(imageId, out var comment)
            ? comment
            : string.Empty;
    }

    private void SetImageReviewComment(Guid imageId, string value)
    {
        _imageReviewComments[imageId] = value;
    }
}
