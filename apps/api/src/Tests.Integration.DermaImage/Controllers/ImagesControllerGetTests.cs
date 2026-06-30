using Domain.DermaImage.Entities;
using Domain.DermaImage.Entities.Enums;
using FluentAssertions;
using Infrastructure.DermaImage;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using Tests.Integration.DermaImage.Infrastructure;

namespace Tests.Integration.DermaImage.Controllers;

/// <summary>
/// Pruebas de integración para el endpoint <c>GET /api/images</c>.
/// Verifican el comportamiento de paginación y control de acceso
/// sin depender de la base de datos real ni de servicios externos.
/// </summary>
public class ImagesControllerGetTests : IClassFixture<DermaImageWebApplicationFactory>, IAsyncLifetime
{
    private readonly DermaImageWebApplicationFactory _factory;
    private readonly HttpClient _anonymousClient;

    public ImagesControllerGetTests(DermaImageWebApplicationFactory factory)
    {
        _factory = factory;
        _anonymousClient = factory.CreateAnonymousClient();
    }

    // ── Setup / Teardown ──────────────────────────────────────────────────────

    public async Task InitializeAsync()
    {
        // Sembrar datos de prueba en la base de datos InMemory
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DermaImageDbContext>();

        db.Images.RemoveRange(db.Images);
        await db.SaveChangesAsync();

        // Crear 25 imágenes públicas
        for (var i = 1; i <= 25; i++)
        {
            db.Images.Add(new DermaImg
            {
                Id = Guid.NewGuid(),
                PublicId = $"DERM_{i:D7}",
                FileName = $"img_{i}.jpg",
                FilePath = $"/uploads/img_{i}.jpg",
                ContentType = "image/jpeg",
                FileSize = 512_000,
                IsPublic = true,
                ContributorId = Guid.NewGuid(),
                AgeApprox = 30 + i,
                Sex = i % 2 == 0 ? Sex.Female : Sex.Male,
                AnatomSiteGeneral = AnatomSiteGeneral.AnteriorTorso,
                Diagnosis = $"Diagnóstico #{i}",
                SunExposure = i % 2 == 0,
                CreatedAt = DateTime.UtcNow.AddDays(-i)
            });
        }

        await db.SaveChangesAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    // ── Pruebas ───────────────────────────────────────────────────────────────

    [Fact(DisplayName = "GET /api/images responde HTTP 200 para usuario anónimo")]
    public async Task GetAll_Anonymous_Returns200()
    {
        var response = await _anonymousClient.GetAsync("/api/images");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact(DisplayName = "GET /api/images para usuario anónimo retorna máximo 10 imágenes en página 1")]
    public async Task GetAll_Anonymous_ReturnsAtMost10ImagesOnPage1()
    {
        var response = await _anonymousClient.GetAsync("/api/images?page=1&pageSize=20");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<PagedResponseDto>();

        body!.Items.Should().HaveCountLessOrEqualTo(10,
            because: "los usuarios anónimos sólo pueden ver hasta 10 imágenes de previsualización");
    }

    [Fact(DisplayName = "GET /api/images para usuario anónimo en página 2 retorna lista vacía")]
    public async Task GetAll_Anonymous_Page2_ReturnsEmptyItems()
    {
        var response = await _anonymousClient.GetAsync("/api/images?page=2&pageSize=20");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<PagedResponseDto>();

        body!.Items.Should().BeEmpty(
            because: "los usuarios anónimos no pueden paginar más allá de la primera página");
    }

    [Fact(DisplayName = "GET /api/images incluye TotalCount correcto aunque se recorten los items")]
    public async Task GetAll_Anonymous_TotalCountReflectsAllPublicImages()
    {
        var response = await _anonymousClient.GetAsync("/api/images?page=1&pageSize=5");
        var body = await response.Content.ReadFromJsonAsync<PagedResponseDto>();

        body!.TotalCount.Should().Be(25,
            because: "hay 25 imágenes públicas sembradas, independientemente del recorte para anónimos");
    }

    [Fact(DisplayName = "GET /api/images/{id} retorna 200 para imagen pública existente")]
    public async Task GetById_WithPublicImage_Returns200()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DermaImageDbContext>();
        var firstImage = db.Images.First();

        var response = await _anonymousClient.GetAsync($"/api/images/{firstImage.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact(DisplayName = "GET /api/images/{id} retorna 404 para ID inexistente")]
    public async Task GetById_WithNonExistentId_Returns404()
    {
        var response = await _anonymousClient.GetAsync($"/api/images/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact(DisplayName = "GET /api/images/public/{publicId} retorna imagen por publicId")]
    public async Task GetByPublicId_WithValidPublicId_Returns200()
    {
        var response = await _anonymousClient.GetAsync("/api/images/public/DERM_0000001");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    // ── DTOs internos para deserializar la respuesta ──────────────────────────

    private record PagedResponseDto(IEnumerable<object> Items, int TotalCount, int Page, int PageSize);
}
