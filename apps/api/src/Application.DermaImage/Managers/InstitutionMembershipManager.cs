using Application.DermaImage.DTOs;
using Domain.DermaImage.Entities;
using Domain.DermaImage.Entities.Enums;
using Domain.DermaImage.Interfaces.Repository;
using Domain.DermaImage.Interfaces.Services;

namespace Application.DermaImage.Managers;

public class InstitutionMembershipManager : IInstitutionMembershipManager
{
    private readonly IUserRepository _users;
    private readonly IInstitutionRepository _institutions;
    private readonly IInstitutionMembershipRepository _membership;
    private readonly IEmailService _email;

    public InstitutionMembershipManager(
        IUserRepository users,
        IInstitutionRepository institutions,
        IInstitutionMembershipRepository membership,
        IEmailService email)
    {
        _users = users;
        _institutions = institutions;
        _membership = membership;
        _email = email;
    }

    public async Task<IReadOnlyList<InstitutionResponsibleResponseDto>> GetInstitutionResponsiblesAsync(Guid institutionId, CancellationToken cancellationToken = default)
    {
        var responsibles = await _membership.GetInstitutionResponsiblesAsync(institutionId, cancellationToken);
        return responsibles
            .Where(x => x.User is not null)
            .Select(x => new InstitutionResponsibleResponseDto
            {
                UserId = x.UserId,
                FirstName = x.User!.FirstName,
                LastName = x.User!.LastName,
                Email = x.User!.Email ?? string.Empty,
                PhoneNumber = x.User!.PhoneNumber,
            })
            .ToList();
    }

    public async Task AddResponsibleAsync(Guid institutionId, Guid userId, Guid actorUserId, CancellationToken cancellationToken = default)
    {
        var institution = await _institutions.GetByIdAsync(institutionId, cancellationToken)
            ?? throw new KeyNotFoundException("Institución no encontrada.");

        var user = await _users.GetByIdAsync(userId, cancellationToken)
            ?? throw new KeyNotFoundException("Usuario no encontrado.");

        if (!user.IsActive)
        {
            throw new InvalidOperationException("No se puede asignar como responsable a un usuario inactivo.");
        }

        var alreadyResponsible = await _membership.IsInstitutionResponsibleAsync(institution.Id, user.Id, cancellationToken);
        if (alreadyResponsible)
        {
            throw new InvalidOperationException("El usuario ya es responsable de esta institución.");
        }

        var assignment = new InstitutionResponsible
        {
            InstitutionId = institution.Id,
            UserId = user.Id,
            AssignedByUserId = actorUserId,
        };

        await _membership.AddInstitutionResponsibleAsync(assignment, cancellationToken);
    }

    public async Task RemoveResponsibleAsync(Guid institutionId, Guid userId, CancellationToken cancellationToken = default)
    {
        var responsibles = await _membership.GetInstitutionResponsiblesAsync(institutionId, cancellationToken);
        var activeCount = responsibles.Count;

        if (activeCount <= 1 && responsibles.Any(x => x.UserId == userId))
        {
            throw new InvalidOperationException("La institución debe mantener al menos un responsable asignado.");
        }

        await _membership.RemoveInstitutionResponsibleAsync(institutionId, userId, cancellationToken);
    }

    public async Task<InstitutionJoinRequestResponseDto> CreateJoinRequestAsync(Guid requesterUserId, CreateInstitutionJoinRequestDto dto, CancellationToken cancellationToken = default)
    {
        var institution = await _institutions.GetByIdAsync(dto.InstitutionId, cancellationToken)
            ?? throw new KeyNotFoundException("Institución no encontrada.");

        var responsibles = await _membership.GetInstitutionResponsiblesAsync(institution.Id, cancellationToken);
        if (responsibles.Count == 0)
        {
            throw new InvalidOperationException("La institución aún no tiene responsables asignados para revisar solicitudes.");
        }

        var applicant = await _users.GetByIdAsync(requesterUserId, cancellationToken)
            ?? throw new KeyNotFoundException("Usuario no encontrado.");

        if (applicant.InstitutionId is not null)
        {
            throw new InvalidOperationException("Ya estás asociado a una institución.");
        }

        if (!applicant.EmailConfirmed)
        {
            throw new InvalidOperationException("Debes tener el correo confirmado antes de solicitar asociación.");
        }

        var hasPending = await _membership.GetPendingJoinRequestAsync(applicant.Id, institution.Id, cancellationToken);
        if (hasPending is not null)
        {
            throw new InvalidOperationException("Ya tienes una solicitud pendiente para esta institución.");
        }

        var request = new InstitutionJoinRequest
        {
            InstitutionId = institution.Id,
            ApplicantUserId = applicant.Id,
            Status = InstitutionJoinRequestStatus.Pending,
        };

        await _membership.AddJoinRequestAsync(request, cancellationToken);
        var created = await _membership.GetJoinRequestByIdAsync(request.Id, cancellationToken)
            ?? throw new InvalidOperationException("No fue posible recuperar la solicitud creada.");

        return MapToDto(created);
    }

    public async Task<(IReadOnlyList<InstitutionJoinRequestResponseDto> Items, int TotalCount)> GetMyJoinRequestsAsync(Guid requesterUserId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var validPage = Math.Max(1, page);
        var validPageSize = Math.Clamp(pageSize, 1, 100);
        var (requests, totalCount) = await _membership.GetJoinRequestsByUserAsync(requesterUserId, validPage, validPageSize, cancellationToken);
        return (requests.Select(MapToDto).ToList(), totalCount);
    }

    public async Task LeaveInstitutionAsync(Guid requesterUserId, CancellationToken cancellationToken = default)
    {
        var user = await _users.GetByIdAsync(requesterUserId, cancellationToken)
            ?? throw new KeyNotFoundException("Usuario no encontrado.");

        if (user.InstitutionId is null)
        {
            throw new InvalidOperationException("No perteneces a ninguna institución.");
        }

        user.InstitutionId = null;
        await _users.UpdateAsync(user, cancellationToken);
    }

    public async Task<(IReadOnlyList<InstitutionJoinRequestResponseDto> Items, int TotalCount)> GetResponsibleInboxAsync(Guid responsibleUserId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var validPage = Math.Max(1, page);
        var validPageSize = Math.Clamp(pageSize, 1, 100);
        var responsibilities = await _membership.GetResponsibilitiesByUserAsync(responsibleUserId, cancellationToken);
        var institutionIds = responsibilities.Select(x => x.InstitutionId);

        var (requests, totalCount) = await _membership.GetInboxJoinRequestsAsync(institutionIds, validPage, validPageSize, cancellationToken);
        return (requests.Select(MapToDto).ToList(), totalCount);
    }

    public async Task<InstitutionJoinRequestResponseDto> ReviewJoinRequestAsync(Guid responsibleUserId, Guid requestId, ReviewInstitutionJoinRequestDto dto, CancellationToken cancellationToken = default)
    {
        var request = await _membership.GetJoinRequestByIdAsync(requestId, cancellationToken)
            ?? throw new KeyNotFoundException("Solicitud no encontrada.");

        var canReview = await _membership.IsInstitutionResponsibleAsync(request.InstitutionId, responsibleUserId, cancellationToken);
        if (!canReview)
        {
            throw new UnauthorizedAccessException("No tienes permisos para revisar esta solicitud.");
        }

        if (request.Status != InstitutionJoinRequestStatus.Pending)
        {
            throw new InvalidOperationException("La solicitud ya fue revisada previamente.");
        }

        request.Status = dto.Approve ? InstitutionJoinRequestStatus.Approved : InstitutionJoinRequestStatus.Denied;
        request.ReviewComment = dto.Comment?.Trim();
        request.ReviewedByUserId = responsibleUserId;
        request.ReviewedAt = DateTime.UtcNow;

        if (dto.Approve)
        {
            var applicant = await _users.GetByIdAsync(request.ApplicantUserId, cancellationToken)
                ?? throw new KeyNotFoundException("Solicitante no encontrado.");

            if (applicant.InstitutionId is not null)
            {
                throw new InvalidOperationException("El solicitante ya pertenece a una institución.");
            }

            applicant.InstitutionId = request.InstitutionId;
            await _users.UpdateAsync(applicant, cancellationToken);
        }

        await _membership.UpdateJoinRequestAsync(request, cancellationToken);

        if (request.ApplicantUser is not null && !string.IsNullOrWhiteSpace(request.ApplicantUser.Email))
        {
            await _email.SendInstitutionJoinRequestReviewedAsync(
                request.ApplicantUser.Email,
                request.ApplicantUser.FirstName,
                request.Institution?.Name ?? string.Empty,
                dto.Approve,
                request.ReviewComment,
                cancellationToken);
        }

        var reviewed = await _membership.GetJoinRequestByIdAsync(request.Id, cancellationToken)
            ?? throw new InvalidOperationException("No fue posible recuperar la solicitud revisada.");

        return MapToDto(reviewed);
    }

    private static InstitutionJoinRequestResponseDto MapToDto(InstitutionJoinRequest request)
    {
        return new InstitutionJoinRequestResponseDto
        {
            Id = request.Id,
            InstitutionId = request.InstitutionId,
            InstitutionName = request.Institution?.Name ?? string.Empty,
            ApplicantUserId = request.ApplicantUserId,
            ApplicantFirstName = request.ApplicantUser?.FirstName ?? string.Empty,
            ApplicantLastName = request.ApplicantUser?.LastName ?? string.Empty,
            ApplicantEmail = request.ApplicantUser?.Email ?? string.Empty,
            ApplicantPhoneNumber = request.ApplicantUser?.PhoneNumber ?? string.Empty,
            Status = request.Status.ToString(),
            ReviewComment = request.ReviewComment,
            ReviewedByUserId = request.ReviewedByUserId,
            ReviewedByFullName = request.ReviewedByUser is null
                ? null
                : $"{request.ReviewedByUser.FirstName} {request.ReviewedByUser.LastName}".Trim(),
            ReviewedAt = request.ReviewedAt,
            CreatedAt = request.CreatedAt,
        };
    }
}