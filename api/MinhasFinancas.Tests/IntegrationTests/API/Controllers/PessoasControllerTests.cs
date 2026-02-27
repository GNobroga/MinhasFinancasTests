using FluentAssertions;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using MinhasFinancas.Application.DTOs;
using MinhasFinancas.Application.Services;
using MinhasFinancas.Domain.Entities;
using MinhasFinancas.Domain.ValueObjects;
using MinhasFinancas.Tests.Common;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.Mime;
using System.Text.Json;

namespace MinhasFinancas.Tests.IntegrationTests.API.Controllers;

[Trait("IntegrationTests", "API - PessoasController")]
public sealed class PessoasControllerTests(WebApplicationTestingFactory factory) : BaseIntegrationTest(factory)
{

    public const string BaseEndpoint = "/api/v1/pessoas";

    public IPessoaService PessoaService => ScopedServices.GetRequiredService<IPessoaService>();

    public PessoaFixture PessoaFixture => Fixtures.PessoaFixture;

    [Theory]
    [InlineData(0, 10)]
    [InlineData(1, 2)]
    [InlineData(2, 1)]
    public async Task GetAll(int page, int pageSize)
    {
        // Arrange
        var queryParams = new Dictionary<string, string?>()
        {
            ["page"] = page.ToString(),
            ["pageSize"] = pageSize.ToString(),
        };
        string uri = QueryHelpers.AddQueryString(BaseEndpoint, queryParams);
    
        // Act
        PagedResult<PessoaDto>? result = await TestClient
                .GetFromJsonAsync<PagedResult<PessoaDto>>(uri);

        // Arrange
        result.Should().NotBeNull();
        result.Page.Should().Be(page);
        result.PageSize.Should().Be(pageSize);
        result.Items.Count().Should().BeLessThanOrEqualTo(pageSize);
    }

    [Fact]
    public async Task GetAll_QuandoInformadoParametroDePesquisa_DeveRetornarApenasPessoasFiltradas()
    {
        // Arrange
        Pessoa pessoa = PessoaFixture.BuildPessoa();

        PessoaDto created = await PessoaService.CreateAsync(new CreatePessoaDto
        {
           Nome = pessoa.Nome,
           DataNascimento = pessoa.DataNascimento, 
        });

        // Act
       
        async Task<bool> NavigatePagesUntilFoundAsync(int page = 1, int pageSize = 100)
        {
            PagedResult<PessoaDto>? result = await TestClient
                .GetFromJsonAsync<PagedResult<PessoaDto>>(
                    EndpointUtils.BuildUrl(BaseEndpoint,queryParams: $"page={page}&pageSize={pageSize}&search={pessoa.Nome}"))
                .ConfigureAwait(false);
                
            ArgumentNullException.ThrowIfNull(result);
            if (!result.Items.Any())
                return false;
            
            if (result.Items.Any(p => p.Id == created.Id)) 
                return true;
            
            return await NavigatePagesUntilFoundAsync(page + 1, pageSize)
                    .ConfigureAwait(false);
        }
        
        bool result = await NavigatePagesUntilFoundAsync();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task GetById_QuandoExistir_DeveRetornarPessoa()
    {
        // Arrange
        Pessoa pessoa = PessoaFixture.BuildPessoa();

        PessoaDto created = await PessoaService.CreateAsync(new CreatePessoaDto
        {
            Nome = pessoa.Nome,
            DataNascimento = pessoa.DataNascimento,
        });

        // Act
        PessoaDto? result = await TestClient.GetFromJsonAsync<PessoaDto?>(
                EndpointUtils.BuildUrl(BaseEndpoint, $"{created.Id}")
            );

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be( created.Id );
        result.Nome.Should().Be(created.Nome);
        result.DataNascimento.Should().Be(created.DataNascimento);
    }

    [Fact]
    public async Task GetById_QuandoNaoExistir_DeveRetornarStatusCode404()
    {
        // Arrange
        Guid id = Guid.NewGuid();

        // Act
        string endpoint = EndpointUtils.BuildUrl(BaseEndpoint, $"{id}");
        HttpResponseMessage response = await TestClient.GetAsync(endpoint);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Create_QuandoDadosSaoValidos_DeveRetornaPessoaDto()
    {
        // Arrange
        Pessoa pessoa = PessoaFixture.BuildPessoa();

        CreatePessoaDto request = new()
        {
            Nome = pessoa.Nome,
            DataNascimento = pessoa.DataNascimento,
        };

        MediaTypeHeaderValue contentType = new(MediaTypeNames.Application.Json);
        StringContent content = new(JsonSerializer.Serialize(request), contentType);
        string url = EndpointUtils.BuildUrl(BaseEndpoint);

        // Act
        HttpResponseMessage response = await TestClient.PostAsync(url, content);
        PessoaDto? result = await response.Content.ReadFromJsonAsync<PessoaDto>();

        DateTime today = DateTime.UtcNow;
        // Arrange
        result.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        result.Nome.Should().Be(request.Nome);
        result.DataNascimento.Should().Be(request.DataNascimento);
        result.Idade.Should().Be(pessoa.Idade);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("  ")]
    public async Task Create_QuandoNomeEhNuloOuEmBranco_DeveRetornarErro400BadRequest(string name)
    {
        // Arrange
        Pessoa pessoa = PessoaFixture.BuildPessoa();

        CreatePessoaDto request = new()
        {
            Nome = name,
            DataNascimento = pessoa.DataNascimento,
        };

        MediaTypeHeaderValue contentType = new(MediaTypeNames.Application.Json);
        StringContent content = new(JsonSerializer.Serialize(request), contentType);
        string url = EndpointUtils.BuildUrl(BaseEndpoint);

        // Act
        HttpResponseMessage response = await TestClient.PostAsync(url, content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Create_QuandoNomeEhMaiorQue200Caracteres_DeveRetornarErro400BadRequest()
    {
        // Arrange
        Pessoa pessoa = PessoaFixture.BuildPessoa();

        CreatePessoaDto request = new()
        {
            Nome = string.Join(string.Empty, Enumerable.Range(0, 201).Select(x => x.ToString())),
            DataNascimento = pessoa.DataNascimento,
        };

        MediaTypeHeaderValue contentType = new(MediaTypeNames.Application.Json);
        StringContent content = new(JsonSerializer.Serialize(request), contentType);
        string url = EndpointUtils.BuildUrl(BaseEndpoint);

        // Act
        HttpResponseMessage response = await TestClient.PostAsync(url, content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Theory]
    [MemberData(nameof(GetInvalidBirthDates))]
    public async Task Create_QuandoDataNascimentoEhNulaOuPadrao_DeveRetornarErro400BadRequest(DateTime dataNascimento)
    {
        // Arrange
        Pessoa pessoa = PessoaFixture.BuildPessoa();

        CreatePessoaDto request = new()
        {
            Nome = pessoa.Nome,
            DataNascimento = dataNascimento
        };

        MediaTypeHeaderValue contentType = new(MediaTypeNames.Application.Json);
        StringContent content = new(JsonSerializer.Serialize(request), contentType);
        string url = EndpointUtils.BuildUrl(BaseEndpoint);

        // Act
        HttpResponseMessage response = await TestClient.PostAsync(url, content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Create_QuandoDataNascimentoEhNoFuturo_DeveRetornarErro400BadRequest()
    {
        // Arrange
        Pessoa pessoa = PessoaFixture.BuildPessoa();

        CreatePessoaDto request = new()
        {
            Nome = pessoa.Nome,
            DataNascimento = DateTime.UtcNow.AddDays(1),
        };

        MediaTypeHeaderValue contentType = new(MediaTypeNames.Application.Json);
        StringContent content = new(JsonSerializer.Serialize(request), contentType);
        string url = EndpointUtils.BuildUrl(BaseEndpoint);

        // Act
        HttpResponseMessage response = await TestClient.PostAsync(url, content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Update_QuandoDadosValidos_DeveAtualizar()
    {
        // Arrange
        Pessoa pessoa = PessoaFixture.BuildPessoa();

        CreatePessoaDto createRequest = new()
        {
            Nome = pessoa.Nome,
            DataNascimento = DateTime.UtcNow.AddDays(1),
        };

        PessoaDto created = await PessoaService.CreateAsync(createRequest);

        UpdatePessoaDto updateRequest = new()
        {
            Nome = "Pessoa atualizada!",
            DataNascimento = DateTime.Parse("2000-04-10"),
        };

        string json = JsonSerializer.Serialize(updateRequest);
        StringContent content = new(json, new MediaTypeHeaderValue(MediaTypeNames.Application.Json));

        // Act
        HttpResponseMessage response = await TestClient.PutAsync(
            EndpointUtils.BuildUrl(
                BaseEndpoint,
                endpoint: $"{created.Id}"
             ),
            content
        );

        PessoaDto? result = await TestClient.GetFromJsonAsync<PessoaDto?>(
              EndpointUtils.BuildUrl(BaseEndpoint, $"{created.Id}")
         );

        // Assert
        result.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        result.DataNascimento.Should().Be(updateRequest.DataNascimento);
        result.Nome.Should().Be(updateRequest.Nome);
    }

    [Fact]
    public async Task Update_QuandoNaoEncontrado_DeveRetornar404NotFound()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        UpdatePessoaDto updateRequest = new()
        {
            Nome = PessoaFixture.BuildPessoa().Nome,
            DataNascimento = PessoaFixture.BuildPessoa().DataNascimento,
        };

        StringContent content = new(
            JsonSerializer.Serialize(updateRequest),
            new MediaTypeHeaderValue(MediaTypeNames.Application.Json)
        );

        // Act

        HttpResponseMessage response = await TestClient.PutAsync(
            EndpointUtils.BuildUrl(BaseEndpoint, endpoint: $"{id}"),
            content
        );

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("  ")]
    public async Task Update_QuandoNomeEhNulo_DeveRetornar404NotFound(string name)
    {
        // Arrange
        Pessoa pessoa = PessoaFixture.BuildPessoa();

        CreatePessoaDto createRequest = new()
        {
            Nome = pessoa.Nome,
            DataNascimento = DateTime.UtcNow.AddDays(1),
        };

        PessoaDto created = await PessoaService.CreateAsync(createRequest);

        UpdatePessoaDto updateRequest = new()
        {
            Nome = name,
            DataNascimento = PessoaFixture.BuildPessoa().DataNascimento,
        };

        StringContent content = new(
            JsonSerializer.Serialize(updateRequest),
            new MediaTypeHeaderValue(MediaTypeNames.Application.Json)
        );

        // Act

        HttpResponseMessage response = await TestClient.PutAsync(
            EndpointUtils.BuildUrl(BaseEndpoint, endpoint: $"{created.Id}"),
            content
        );

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Theory]
    [MemberData(nameof(GetInvalidBirthDates))]
    public async Task Update_QuandoDataNascimentoEhInvalida_DeveRetornar400BadRequest(DateTime birthDate)
    {
        // Arrange
        Pessoa pessoa = PessoaFixture.BuildPessoa();

        CreatePessoaDto createRequest = new()
        {
            Nome = pessoa.Nome,
            DataNascimento = DateTime.UtcNow.AddDays(1),
        };

        PessoaDto created = await PessoaService.CreateAsync(createRequest);

        UpdatePessoaDto updateRequest = new()
        {
            Nome = pessoa.Nome,
            DataNascimento = birthDate,
        };

        StringContent content = new(
            JsonSerializer.Serialize(updateRequest),
            new MediaTypeHeaderValue(MediaTypeNames.Application.Json)
        );

        // Act

        HttpResponseMessage response = await TestClient.PutAsync(
            EndpointUtils.BuildUrl(BaseEndpoint, endpoint: $"{created.Id}"),
            content
        );

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Update_QuandoDataNascimentoEhFutura_DeveRetornar400BadRequest()
    {
        // Arrange
        Pessoa pessoa = PessoaFixture.BuildPessoa();

        CreatePessoaDto createRequest = new()
        {
            Nome = pessoa.Nome,
            DataNascimento = DateTime.UtcNow.AddDays(1),
        };

        PessoaDto created = await PessoaService.CreateAsync(createRequest);

        UpdatePessoaDto updateRequest = new()
        {
            Nome = pessoa.Nome,
            DataNascimento = DateTime.UtcNow.AddDays(1),
        };

        StringContent content = new(
            JsonSerializer.Serialize(updateRequest),
            new MediaTypeHeaderValue(MediaTypeNames.Application.Json)
        );

        // Act
        HttpResponseMessage response = await TestClient.PutAsync(
            EndpointUtils.BuildUrl(BaseEndpoint, endpoint: $"{created.Id}"),
            content
        );

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Delete()
    {
        // Arrange
        Pessoa pessoa = PessoaFixture.BuildPessoa();

        CreatePessoaDto createRequest = new()
        {
            Nome = pessoa.Nome,
            DataNascimento = DateTime.UtcNow.AddDays(1),
        };

        PessoaDto created = await PessoaService.CreateAsync(createRequest);

        // Act
        HttpResponseMessage response = await TestClient.DeleteAsync(
            EndpointUtils.BuildUrl(BaseEndpoint, endpoint: $"{created.Id}")
        );

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }


    [Fact]
    public async Task Delete_QuandoNaoEncontrar_DeveRetornar404NotFound()
    {
        // Arrange
        Guid id = Guid.NewGuid();

        // Act
        HttpResponseMessage response = await TestClient.DeleteAsync(
            EndpointUtils.BuildUrl(BaseEndpoint, endpoint: $"{id}")
        );

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }


    public static IEnumerable<object[]> GetInvalidBirthDates() => new List<object[]>()
    {
        new object[] {  DateTime.MinValue },
        new object[] { null }
    };

}
