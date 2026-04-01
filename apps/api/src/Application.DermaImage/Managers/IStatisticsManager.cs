using Application.DermaImage.DTOs;

namespace Application.DermaImage.Managers;

public interface IStatisticsManager
{
    Task<StatisticsOverviewDto> GetOverviewAsync(
        bool includePrivate,
        int recentMonths = 6,
        int topInstitutions = 5,
        CancellationToken cancellationToken = default);
}
