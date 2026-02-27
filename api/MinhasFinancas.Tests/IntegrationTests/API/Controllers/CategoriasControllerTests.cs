using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using MinhasFinancas.Application.DTOs;
using MinhasFinancas.Application.Services;
using MinhasFinancas.Domain.Entities;
using MinhasFinancas.Tests.Common;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.Mime;
using System.Text.Json;


namespace MinhasFinancas.Tests.IntegrationTests.API.Controllers;

[Trait("IntegrationTests", "API - CategoriasController")]
public sealed class CategoriasControllerTests(WebApplicationTestingFactory factory) : BaseIntegrationTest(factory)
{
    public ICategoriaService CategoriaService => ScopedServices.GetRequiredService<ICategoriaService>();

    public CategoriaFixture CategoriaFixture => Fixtures.CategoriaFixture;

    public const string BaseEndpoint = "/api/v1/categorias";

    [Fact]
    public async Task Create()
    {
        // Arrange
        CreateCategoriaDto createRequest = new()
        {
            Descricao = string.Join(string.Empty, Faker.Lorem.Words(4)),
            Finalidade = Faker.Random.Enum<Categoria.EFinalidade>(),
        };

        StringContent content = new(
            JsonSerializer.Serialize(createRequest),
            new MediaTypeHeaderValue(MediaTypeNames.Application.Json)
        );

        // Act
        HttpResponseMessage response = await TestClient.PostAsync(BaseEndpoint, content);

        CategoriaDto? categoria = await response.Content.ReadFromJsonAsync<CategoriaDto>();
        // Assert
        categoria.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        categoria.Descricao.Should().Be(createRequest.Descricao);
        categoria.Finalidade.Should().Be(createRequest.Finalidade);
    }

    [Theory]
    [InlineData(" ")]
    [InlineData(null)]
    [InlineData("   ")]
    public async Task Create_QuandoDescricaoEhVazia_DeveRetornar400BadRequest(string description)
    {
        // Arrange
        CreateCategoriaDto createRequest = new()
        {
            Descricao = description,
            Finalidade = Faker.Random.Enum<Categoria.EFinalidade>(),
        };

        StringContent content = new(
            JsonSerializer.Serialize(createRequest),
            new MediaTypeHeaderValue(MediaTypeNames.Application.Json)
        );

        string url = EndpointUtils.BuildUrl(BaseEndpoint);

        // Act
        HttpResponseMessage response = await TestClient.PostAsync(url, content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Create_QuandoDescricaoEhMaiorQue200Caracteres_DeveRetornar400BadRequest()
    {
        // Arrange
        CreateCategoriaDto createRequest = new()
        {
            Descricao = string.Join(string.Empty, Enumerable.Range(0, 201).Select(x => x.ToString())),
            Finalidade = Faker.Random.Enum<Categoria.EFinalidade>(),
        };

        StringContent content = new(
            JsonSerializer.Serialize(createRequest),
            new MediaTypeHeaderValue(MediaTypeNames.Application.Json)
        );

        string url = EndpointUtils.BuildUrl(BaseEndpoint);

        // Act
        HttpResponseMessage response = await TestClient.PostAsync(url, content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
