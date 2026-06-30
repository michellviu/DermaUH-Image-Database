using Application.DermaImage.DTOs;
using Application.DermaImage.Managers;
using Domain.DermaImage.Entities;
using Domain.DermaImage.Entities.Enums;
using Domain.DermaImage.Interfaces.Services;
using FluentAssertions;
using Moq;

namespace Tests.Unit.DermaImage.Managers;

/// <summary>
/// Pruebas unitarias para <see cref="DermaImgManager"/>.
/// Aíslan la lógica de orquestación del manager del dominio,
/// reemplazando las dependencias con mocks.
/// </summary>
public class DermaImgManagerTests
{
    private readonly Mock<IDermaImgService> _serviceMock;
    private readonly Mock<IInstitutionManager> _institutionManagerMock;
    private readonly DermaImgManager _sut;

    public DermaImgManagerTests()
    {
        _serviceMock = new Mock<IDermaImgService>(MockBehavior.Strict);
        _institutionManagerMock = new Mock<IInstitutionManager>(MockBehavior.Strict);
        _sut = new DermaImgManager(_serviceMock.Object, _institutionManagerMock.Object);
    }

    // ── CreateAsync ──────────────────────────────────────────────────────────

    [Fact(DisplayName = "CreateAsync sin institución no llama a GetOrCreateAsync")]
    public async Task CreateAsync_WithoutInstitution_DoesNotCallInstitutionManager()
    {
        var dto = BuildMinimalDto(institutionName: null);
        var expectedImage = BuildSampleImage();

        _serviceMock
            .Setup(s => s.CreateAsync(It.IsAny<DermaImg>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedImage);

        var result = await _sut.CreateAsync(dto);

        result.Should().BeEquivalentTo(expectedImage);
        _institutionManagerMock.Verify(
            m => m.GetOrCreateAsync(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact(DisplayName = "CreateAsync con institución llama a GetOrCreateAsync y asigna InstitutionId")]
    public async Task CreateAsync_WithInstitutionName_CallsGetOrCreateAndSetsId()
    {
        var institutionId = Guid.NewGuid();
        var institution = new Institution { Id = institutionId, Name = "Hospital Calixto García" };
        var dto = BuildMinimalDto(institutionName: "Hospital Calixto García");
        var expectedImage = BuildSampleImage();
        expectedImage.InstitutionId = institutionId;

        _institutionManagerMock
            .Setup(m => m.GetOrCreateAsync("Hospital Calixto García", null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(institution);

        _serviceMock
            .Setup(s => s.CreateAsync(It.Is<DermaImg>(img => img.InstitutionId == institutionId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedImage);

        var result = await _sut.CreateAsync(dto);

        result.InstitutionId.Should().Be(institutionId);
        _institutionManagerMock.Verify(
            m => m.GetOrCreateAsync("Hospital Calixto García", null, null, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact(DisplayName = "CreateAsync propaga excepción cuando el servicio falla")]
    public async Task CreateAsync_WhenServiceThrows_PropagatesException()
    {
        var dto = BuildMinimalDto();

        _serviceMock
            .Setup(s => s.CreateAsync(It.IsAny<DermaImg>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("DB error"));

        var act = async () => await _sut.CreateAsync(dto);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("DB error");
    }

    // ── GetByIdAsync ─────────────────────────────────────────────────────────

    [Fact(DisplayName = "GetByIdAsync retorna null cuando la imagen no existe")]
    public async Task GetByIdAsync_WhenNotFound_ReturnsNull()
    {
        var id = Guid.NewGuid();
        _serviceMock.Setup(s => s.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((DermaImg?)null);

        var result = await _sut.GetByIdAsync(id);

        result.Should().BeNull();
    }

    [Fact(DisplayName = "GetByIdAsync retorna imagen existente")]
    public async Task GetByIdAsync_WhenFound_ReturnsImage()
    {
        var image = BuildSampleImage();
        _serviceMock.Setup(s => s.GetByIdAsync(image.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(image);

        var result = await _sut.GetByIdAsync(image.Id);

        result.Should().BeEquivalentTo(image);
    }

    // ── UpdateAsync ──────────────────────────────────────────────────────────

    [Fact(DisplayName = "UpdateAsync lanza KeyNotFoundException si la imagen no existe")]
    public async Task UpdateAsync_WhenImageNotFound_ThrowsKeyNotFoundException()
    {
        var id = Guid.NewGuid();
        _serviceMock.Setup(s => s.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((DermaImg?)null);

        var act = async () => await _sut.UpdateAsync(id, BuildMinimalDto());

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"*{id}*");
    }

    [Fact(DisplayName = "UpdateAsync llama a UpdateAsync del servicio cuando la imagen existe")]
    public async Task UpdateAsync_WhenImageExists_CallsServiceUpdate()
    {
        var existing = BuildSampleImage();
        var dto = BuildMinimalDto();

        _serviceMock.Setup(s => s.GetByIdAsync(existing.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);
        _serviceMock.Setup(s => s.UpdateAsync(existing, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await _sut.UpdateAsync(existing.Id, dto);

        _serviceMock.Verify(s => s.UpdateAsync(existing, It.IsAny<CancellationToken>()), Times.Once);
    }

    // ── DeleteAsync ──────────────────────────────────────────────────────────

    [Fact(DisplayName = "DeleteAsync delega en el servicio correctamente")]
    public async Task DeleteAsync_CallsServiceDeleteAsync()
    {
        var id = Guid.NewGuid();
        _serviceMock.Setup(s => s.DeleteAsync(id, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await _sut.DeleteAsync(id);

        _serviceMock.Verify(s => s.DeleteAsync(id, It.IsAny<CancellationToken>()), Times.Once);
    }

    // ── GetPagedAsync ────────────────────────────────────────────────────────

    [Fact(DisplayName = "GetPagedAsync retorna resultado paginado del servicio")]
    public async Task GetPagedAsync_ReturnsPagedResultFromService()
    {
        var images = new List<DermaImg> { BuildSampleImage(), BuildSampleImage() };
        var expected = (Items: (IEnumerable<DermaImg>)images, TotalCount: 2);

        _serviceMock
            .Setup(s => s.GetPagedAsync(1, 20, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await _sut.GetPagedAsync(1, 20);

        result.TotalCount.Should().Be(2);
        result.Items.Should().HaveCount(2);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static CreateDermaImgDto BuildMinimalDto(string? institutionName = null) => new()
    {
        FileName = "lesion.jpg",
        FilePath = "/uploads/lesion.jpg",
        ContentType = "image/jpeg",
        FileSize = 204_800,
        ContributorId = Guid.NewGuid(),
        AgeApprox = 45,
        Sex = Sex.Female,
        AnatomSiteGeneral = AnatomSiteGeneral.AnteriorTorso,
        Diagnosis = "Melanoma superficial",
        SunExposure = true,
        InstitutionName = institutionName
    };

    private static DermaImg BuildSampleImage() => new()
    {
        Id = Guid.NewGuid(),
        PublicId = $"DERM_{Guid.NewGuid():N}",
        FileName = "sample.jpg",
        FilePath = "/uploads/sample.jpg",
        ContentType = "image/jpeg",
        FileSize = 512_000,
        IsPublic = true,
        ContributorId = Guid.NewGuid()
    };
}
