using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using MinhasFinancas.Application.DTOs;
using MinhasFinancas.Application.Services;
using MinhasFinancas.Domain.Entities;
using MinhasFinancas.Domain.Interfaces;
using MinhasFinancas.Tests.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MinhasFinancas.Tests.IntegrationTests.API.Controllers;

[Trait("IntegrationTests", "API - TransacoesController")]
public sealed class TransacoesControllerTests(WebApplicationTestingFactory factory) : BaseIntegrationTest(factory)
{
    public const string BaseEndpoint = "/api/v1/transacoes";
    public ITransacaoService TransacaoService => ScopedServices.GetRequiredService<ITransacaoService>();

    public TransacaoFixture TransacaoFixture => Fixtures.TransacaoFixture;

    [Fact]
    public async Task Create()
    {
        // Arrange
        Transacao.ETipo tipoTransacao = Faker.Random.Enum<Transacao.ETipo>();

        Categoria.EFinalidade finalidadeCategoria = tipoTransacao switch
        {
            Transacao.ETipo.Receita => Categoria.EFinalidade.Receita,
            Transacao.ETipo.Despesa => Categoria.EFinalidade.Despesa,
            _ => throw new ArgumentException()
        };

        Guid categoriaId = DbContext.Categorias
            .First(c => c.Finalidade == finalidadeCategoria)
            .Id;

        Guid pessoaId = DbContext.Pessoas
            .OrderBy(p => p.DataNascimento)
            .AsEnumerable()
            .First(p => p.Idade > 17)
            .Id;

        CreateTransacaoDto request = new()
        {
            Descricao = Faker.Random.String2(100),
            Valor = Faker.Random.Decimal(1, 1000),
            Tipo = tipoTransacao,
            CategoriaId = categoriaId,
            PessoaId = pessoaId,
            Data = DateTime.UtcNow
        };

        StringContent content = new(
                JsonSerializer.Serialize(request),
                new System.Net.Http.Headers.MediaTypeHeaderValue(MediaTypeNames.Application.Json)
        );

        // Act
        HttpResponseMessage response = await TestClient.PostAsync(BaseEndpoint, content);
        TransacaoDto? transacao = await response.Content.ReadFromJsonAsync<TransacaoDto>();

        // Assert
        transacao.Should().NotBeNull();
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);
        transacao.Descricao.Should().Be(request.Descricao);
        transacao.CategoriaId.Should().Be(request.CategoriaId);
        transacao.PessoaId.Should().Be(request.PessoaId);
    }

    [Fact]
    public async Task Create_QuandoCategoriaFinalidadeForDiferenteDoTipoTransacao_DeveRetornar400BadRequest()
    {
        // Arrange
        Transacao.ETipo tipoTransacao = Faker.Random.Enum<Transacao.ETipo>();

        Categoria.EFinalidade finalidadeCategoria = tipoTransacao switch
        {
            Transacao.ETipo.Receita => Categoria.EFinalidade.Despesa,
            Transacao.ETipo.Despesa => Categoria.EFinalidade.Receita,
            _ => throw new ArgumentException()
        };

        Guid categoriaId = DbContext.Categorias
            .First(c => c.Finalidade == finalidadeCategoria)
            .Id;

        Guid pessoaId = DbContext.Pessoas
            .OrderBy(p => p.DataNascimento)
            .AsEnumerable()
            .First(p => p.Idade > 17)
            .Id;

        CreateTransacaoDto request = new()
        {
            Descricao = Faker.Random.String2(100),
            Valor = Faker.Random.Decimal(1, 1000),
            Tipo = tipoTransacao,
            CategoriaId = categoriaId,
            PessoaId = pessoaId,
            Data = DateTime.UtcNow
        };

        StringContent content = new(
                JsonSerializer.Serialize(request),
                new System.Net.Http.Headers.MediaTypeHeaderValue(MediaTypeNames.Application.Json)
        );

        // Act
        HttpResponseMessage response = await TestClient.PostAsync(BaseEndpoint, content);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }


    [Theory]
    [InlineData(nameof(Pessoa))]
    [InlineData(nameof(Categoria))]
    public async Task Create_QuandoCategoriaOuPessoaNaoExistir_DeveRetornar400BadRequest(string entidade)
    {
        // Arrange
        Transacao.ETipo tipoTransacao = Faker.Random.Enum<Transacao.ETipo>();

        Categoria.EFinalidade finalidadeCategoria = tipoTransacao switch
        {
            Transacao.ETipo.Receita => Categoria.EFinalidade.Receita,
            Transacao.ETipo.Despesa => Categoria.EFinalidade.Despesa,
            _ => throw new ArgumentException()
        };

        Guid categoriaId = entidade == nameof(Categoria) ?
                Guid.NewGuid() :
                    DbContext.Categorias.First(c => c.Finalidade == finalidadeCategoria).Id;

        Guid pessoaId = entidade == nameof(Pessoa) ?
             Guid.NewGuid() :
                 DbContext.Pessoas
                    .OrderBy(p => p.DataNascimento)
                    .AsEnumerable()
                    .First(p => p.Idade > 17)
                    .Id;

        CreateTransacaoDto request = new()
        {
            Descricao = Faker.Random.String2(100),
            Valor = Faker.Random.Decimal(1, 1000),
            Tipo = tipoTransacao,
            CategoriaId = categoriaId,
            PessoaId = pessoaId,
            Data = DateTime.UtcNow
        };

        StringContent content = new(
                JsonSerializer.Serialize(request),
                new System.Net.Http.Headers.MediaTypeHeaderValue(MediaTypeNames.Application.Json)
        );

        // Act
        HttpResponseMessage response = await TestClient.PostAsync(BaseEndpoint, content);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Theory]
    [InlineData(" ")]
    [InlineData("    ")]
    [InlineData(null)]
    public async Task Create_QuandoDescricaoEhNula_DeveRetornar400BadRequest(string? descricao)
    {
        // Arrange
        Transacao.ETipo tipoTransacao = Faker.Random.Enum<Transacao.ETipo>();

        Categoria.EFinalidade finalidadeCategoria = tipoTransacao switch
        {
            Transacao.ETipo.Receita => Categoria.EFinalidade.Despesa,
            Transacao.ETipo.Despesa => Categoria.EFinalidade.Receita,
            _ => throw new ArgumentException()
        };

        Guid categoriaId = DbContext.Categorias
            .First(c => c.Finalidade == finalidadeCategoria)
            .Id;

        Guid pessoaId = DbContext.Pessoas
            .OrderBy(p => p.DataNascimento)
            .AsEnumerable()
            .First(p => p.Idade > 17)
            .Id;

        CreateTransacaoDto request = new()
        {
            Descricao = descricao!,
            Valor = Faker.Random.Decimal(1, 1000),
            Tipo = tipoTransacao,
            CategoriaId = categoriaId,
            PessoaId = pessoaId,
            Data = DateTime.UtcNow
        };

        StringContent content = new(
                JsonSerializer.Serialize(request),
                new System.Net.Http.Headers.MediaTypeHeaderValue(MediaTypeNames.Application.Json)
        );

        // Act
        HttpResponseMessage response = await TestClient.PostAsync(BaseEndpoint, content);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }


    [Fact]
    public async Task Create_QuandoDescricaoEhMaiorQue200Caracteres_DeveRetornar400BadRequest()
    {
        // Arrange
        Transacao.ETipo tipoTransacao = Faker.Random.Enum<Transacao.ETipo>();

        Categoria.EFinalidade finalidadeCategoria = tipoTransacao switch
        {
            Transacao.ETipo.Receita => Categoria.EFinalidade.Despesa,
            Transacao.ETipo.Despesa => Categoria.EFinalidade.Receita,
            _ => throw new ArgumentException()
        };

        Guid categoriaId = DbContext.Categorias
            .First(c => c.Finalidade == finalidadeCategoria)
            .Id;

        Guid pessoaId = DbContext.Pessoas
            .OrderBy(p => p.DataNascimento)
            .AsEnumerable()
            .First(p => p.Idade > 17)
            .Id;

        CreateTransacaoDto request = new()
        {
            Descricao = Faker.Random.Words(300),
            Valor = Faker.Random.Decimal(1, 1000),
            Tipo = tipoTransacao,
            CategoriaId = categoriaId,
            PessoaId = pessoaId,
            Data = DateTime.UtcNow
        };

        StringContent content = new(
                JsonSerializer.Serialize(request),
                new System.Net.Http.Headers.MediaTypeHeaderValue(MediaTypeNames.Application.Json)
        );

        // Act
        HttpResponseMessage response = await TestClient.PostAsync(BaseEndpoint, content);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Create_QuandoCategoriaIdForNula_DeveRetornar400BadRequest()
    {
        // Arrange
        Transacao.ETipo tipoTransacao = Faker.Random.Enum<Transacao.ETipo>();

        Categoria.EFinalidade finalidadeCategoria = tipoTransacao switch
        {
            Transacao.ETipo.Receita => Categoria.EFinalidade.Despesa,
            Transacao.ETipo.Despesa => Categoria.EFinalidade.Receita,
            _ => throw new ArgumentException()
        };

        Guid pessoaId = DbContext.Pessoas
            .OrderBy(p => p.DataNascimento)
            .AsEnumerable()
            .First(p => p.Idade > 17)
            .Id;

        CreateTransacaoDto request = new()
        {
            Descricao = Faker.Random.Words(300),
            Valor = Faker.Random.Decimal(1, 1000),
            Tipo = tipoTransacao,
            PessoaId = pessoaId,
            Data = DateTime.UtcNow
        };

        StringContent content = new(
                JsonSerializer.Serialize(request),
                new System.Net.Http.Headers.MediaTypeHeaderValue(MediaTypeNames.Application.Json)
        );

        // Act
        HttpResponseMessage response = await TestClient.PostAsync(BaseEndpoint, content);

        var conten = await  response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }
}
