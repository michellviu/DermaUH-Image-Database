using Application.DermaImage.Services;
using Domain.DermaImage.Entities;
using Domain.DermaImage.Entities.Enums;
using Domain.DermaImage.Interfaces.Repository;
using FluentAssertions;
using Moq;

namespace Tests.Unit.DermaImage.Services;

/// <summary>
/// Pruebas unitarias para <see cref="DermaImgService"/>.
/// Verifican que el servicio delegue correctamente en el repositorio
/// y aplique la lógica de generación del <c>PublicId</c> durante la creación.
/// </summary>
public class DermaImgServiceTests
{
    private readonly Mock<IDermaImgRepository> _repoMock;
    private readonly DermaImgService _sut;

    public DermaImgServiceTests()
    {
        _repoMock = new Mock<IDermaImgRepository>(MockBehavior.Strict);
        _sut = new DermaImgService(_repoMock.Object);
    }

    // ── CreateAsync ──────────────────────────────────────────────────────────

    [Fact(DisplayName = "CreateAsync genera PublicId antes de persistir")]
    public async Task CreateAsync_GeneratesPublicIdBeforeAddAsync()
    {
        const string generatedPublicId = "DERM_0000001";
        var image = BuildImage();

        _repoMock.Setup(r => r.GeneratePublicIdAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(generatedPublicId);

        _repoMock.Setup(r => r.AddAsync(It.Is<DermaImg>(img => img.PublicId == generatedPublicId), It.IsAny<CancellationToken>()))
            .ReturnsAsync((DermaImg img, CancellationToken _) => img);

        var result = await _sut.CreateAsync(image);

        result.PublicId.Should().Be(generatedPublicId);
        _repoMock.Verify(r => r.GeneratePublicIdAsync(It.IsAny<CancellationToken>()), Times.Once);
        _repoMock.Verify(r => r.AddAsync(It.IsAny<DermaImg>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "CreateAsync propaga excepción cuando el repositorio falla")]
    public async Task CreateAsync_WhenRepositoryThrows_PropagatesException()
    {
        var image = BuildImage();

        _repoMock.Setup(r => r.GeneratePublicIdAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new TimeoutException("Connection timeout"));

        var act = async () => await _sut.CreateAsync(image);

        await act.Should().ThrowAsync<TimeoutException>()
            .WithMessage("Connection timeout");
    }

    // ── GetByIdAsync ─────────────────────────────────────────────────────────

    [Fact(DisplayName = "GetByIdAsync retorna null para ID inexistente")]
    public async Task GetByIdAsync_WhenNotFound_ReturnsNull()
    {
        var id = Guid.NewGuid();
        _repoMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((DermaImg?)null);

        var result = await _sut.GetByIdAsync(id);

        result.Should().BeNull();
    }

    [Fact(DisplayName = "GetByIdAsync retorna la imagen cuando existe")]
    public async Task GetByIdAsync_WhenFound_ReturnsImage()
    {
        var image = BuildImage();
        _repoMock.Setup(r => r.GetByIdAsync(image.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(image);

        var result = await _sut.GetByIdAsync(image.Id);

        result.Should().BeEquivalentTo(image);
    }

    // ── GetByPublicIdAsync ───────────────────────────────────────────────────

    [Fact(DisplayName = "GetByPublicIdAsync retorna null para PublicId inexistente")]
    public async Task GetByPublicIdAsync_WhenNotFound_ReturnsNull()
    {
        _repoMock.Setup(r => r.GetByPublicIdAsync("DERM_9999999", It.IsAny<CancellationToken>()))
            .ReturnsAsync((DermaImg?)null);

        var result = await _sut.GetByPublicIdAsync("DERM_9999999");

        result.Should().BeNull();
    }

    // ── DeleteAsync ──────────────────────────────────────────────────────────

    [Fact(DisplayName = "DeleteAsync delega en el repositorio sin lógica adicional")]
    public async Task DeleteAsync_DelegatesToRepository()
    {
        var id = Guid.NewGuid();
        _repoMock.Setup(r => r.DeleteAsync(id, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await _sut.DeleteAsync(id);

        _repoMock.Verify(r => r.DeleteAsync(id, It.IsAny<CancellationToken>()), Times.Once);
    }

    // ── UpdateAsync ──────────────────────────────────────────────────────────

    [Fact(DisplayName = "UpdateAsync delega en el repositorio correctamente")]
    public async Task UpdateAsync_DelegatesToRepository()
    {
        var image = BuildImage();
        _repoMock.Setup(r => r.UpdateAsync(image, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await _sut.UpdateAsync(image);

        _repoMock.Verify(r => r.UpdateAsync(image, It.IsAny<CancellationToken>()), Times.Once);
    }

    // ── GetByIdsAsync ────────────────────────────────────────────────────────

    [Fact(DisplayName = "GetByIdsAsync retorna lista vacía para colección vacía de IDs")]
    public async Task GetByIdsAsync_WithEmptyIds_ReturnsEmptyList()
    {
        var ids = Array.Empty<Guid>();
        _repoMock.Setup(r => r.GetByIdsAsync(ids, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<DermaImg>().AsReadOnly());

        var result = await _sut.GetByIdsAsync(ids);

        result.Should().BeEmpty();
    }

    // ── GetByContributorAsync ────────────────────────────────────────────────

    [Fact(DisplayName = "GetByContributorAsync retorna imágenes del contribuidor dado")]
    public async Task GetByContributorAsync_ReturnsContributorImages()
    {
        var contributorId = Guid.NewGuid();
        var images = new List<DermaImg> { BuildImage(contributorId), BuildImage(contributorId) };

        _repoMock.Setup(r => r.GetByContributorIdAsync(contributorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(images);

        var result = await _sut.GetByContributorAsync(contributorId);

        result.Should().HaveCount(2).And.AllSatisfy(img => img.ContributorId.Should().Be(contributorId));
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static DermaImg BuildImage(Guid? contributorId = null) => new()
    {
        Id = Guid.NewGuid(),
        PublicId = string.Empty,
        FileName = "image.jpg",
        FilePath = "/uploads/image.jpg",
        ContentType = "image/jpeg",
        FileSize = 256_000,
        ContributorId = contributorId ?? Guid.NewGuid(),
        IsPublic = true,
        AgeApprox = 40,
        Sex = Sex.Male,
        AnatomSiteGeneral = AnatomSiteGeneral.LowerExtremity,
        Diagnosis = "Queratosis seborreica",
        SunExposure = false
    };
}
