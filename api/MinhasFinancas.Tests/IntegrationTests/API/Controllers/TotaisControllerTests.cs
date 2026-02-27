using FluentAssertions;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MinhasFinancas.Application.DTOs;
using MinhasFinancas.Application.Services;
using MinhasFinancas.Domain.Entities;
using MinhasFinancas.Domain.ValueObjects;
using MinhasFinancas.Tests.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace MinhasFinancas.Tests.IntegrationTests.API.Controllers;

public sealed class TotaisControllerTests(WebApplicationTestingFactory factory) : BaseIntegrationTest(factory)
{

    public const string BaseEndpoint = "/api/v1/totais";

    public ITransacaoService TransacaoService => ScopedServices.GetRequiredService<ITransacaoService>();

    public IPessoaService PessoaService => ScopedServices.GetRequiredService<IPessoaService>();

    [Fact]
    public async Task GetTotaisPorPessoa()
    {
        // Arrange
        var pessoaIds = DbContext.Pessoas
            .Select(p => p.Id)
            .ToList();
     
        // Act
        List<TotalPorPessoa> totalPorPessoas = await GetTotaisPorPessoaAsync();

        // Assert
        totalPorPessoas.Select(tp => tp.PessoaId).Should().Contain(pessoaIds);
    }

    [Fact]
    public async Task GetTotaisPorPessoa_QuandoFiltradoPorPessoaId_DeveRetornarTotalDaPessoa()
    {
        // Arrange
        Guid pessoaId = DbContext.Pessoas
            .Where(p => DbContext.Transacoes.Count(t => t.PessoaId == p.Id) > 1)
            .First().Id;

        Dictionary<Transacao.ETipo, decimal> totalDespesas = DbContext.Transacoes
            .Where(t => t.PessoaId == pessoaId)
            .GroupBy(t => t.Tipo)
            .Select(g => new
            {
                Tipo = g.Key,
                Total = g.Sum(t => t.Valor)
            })
            .ToDictionary(o => o.Tipo, o => o.Total);

        decimal saldo = totalDespesas[Transacao.ETipo.Receita] - totalDespesas[Transacao.ETipo.Despesa];

        // Act
        List<TotalPorPessoa> totalPorPessoas = await GetTotaisPorPessoaAsync(new()
        {
            ["Pessoa.Id"] = pessoaId.ToString()
        });

        // Assert
        totalPorPessoas.First().TotalDespesas.Should().Be(totalDespesas[Transacao.ETipo.Despesa]);
        totalPorPessoas.First().TotalReceitas.Should().Be(totalDespesas[Transacao.ETipo.Receita]);
        totalPorPessoas.First().Saldo.Should().Be(saldo);
    }

    [Fact]
    public async Task GetTotaisPorPessoa_QuandoDataFilterEhPassado_DeveRetornarTransacoesDentroDoIntervalo()
    {
        // Arrange
        Categoria categoria = DbContext
                .Categorias.First(c => c.Finalidade == Categoria.EFinalidade.Receita);

        Pessoa pessoa = DbContext
                .Pessoas
                .AsEnumerable()
                .First(c => c.Idade > 17);

        TransacaoDto transacao = await TransacaoService.CreateAsync(new Application.DTOs.CreateTransacaoDto
        {
            CategoriaId = categoria.Id,
            PessoaId = pessoa.Id,
            Descricao = "Criada agora",
            Tipo = Transacao.ETipo.Receita,
            Valor = 2000,
            Data = new DateTime(2000, 4, 10),
        });


        // Act
        List<TotalPorPessoa> totalPorPessoas = await GetTotaisPorPessoaAsync(new()
        {
            ["Periodo.DataInicio"] = new DateTime(2000, 4, 9).ToString("s"),
            ["Periodo.DataFim"] = new DateTime(2000, 4, 11).ToString("s"),
            ["Pessoa.Id"] = transacao.PessoaId.ToString(),
        });

        // Arrange
        totalPorPessoas.First().Saldo.Should().Be(transacao.Valor);
    }

    [Theory]
    [InlineData(10, 10)]
    [InlineData(9, 10)]
    public async Task GetTotaisPorPessoa_QuandoApenasDataInicioForPassado_DeveRetornarTransacoesDentroDoIntervalo(int transacaoDay, int dataInicioDay)
    {
        // Arrange
        Categoria categoria = DbContext
                .Categorias.First(c => c.Finalidade == Categoria.EFinalidade.Receita);

        PessoaDto pessoa = await PessoaService.CreateAsync(new CreatePessoaDto
        {
            Nome = Faker.Person.FullName,
            DataNascimento = Faker.Person.DateOfBirth
        });

        TransacaoDto transacao = await TransacaoService.CreateAsync(new Application.DTOs.CreateTransacaoDto
        {
            CategoriaId = categoria.Id,
            PessoaId = pessoa.Id,
            Descricao = "Criada agora",
            Tipo = Transacao.ETipo.Receita,
            Valor = 2000,
            Data = new DateTime(2000, 4, transacaoDay),
        });


        // Act
        List<TotalPorPessoa> totalPorPessoas = await GetTotaisPorPessoaAsync(new()
        {
            ["Periodo.DataInicio"] = new DateTime(2000, 4, dataInicioDay).ToString("s"),
            ["Pessoa.Id"] = transacao.PessoaId.ToString(),
        });

        // Arrange
        totalPorPessoas.Select(tp => tp.PessoaId).Should().Contain(pessoa.Id);
    }

    [Theory]
    [InlineData(10, 10)]
    [InlineData(10, 11)]
    public async Task GetTotaisPorPessoa_QuandoApenasDataFimForPassado_DeveRetornarTransacoesDentroDoIntervalo(int transacaoDay, int dataFimDay)
    {
        // Arrange
        Categoria categoria = DbContext
                .Categorias.First(c => c.Finalidade == Categoria.EFinalidade.Receita);

        PessoaDto pessoa = await PessoaService.CreateAsync(new CreatePessoaDto
        {
            Nome = Faker.Person.FullName,
            DataNascimento = Faker.Person.DateOfBirth
        });

        TransacaoDto transacao = await TransacaoService.CreateAsync(new Application.DTOs.CreateTransacaoDto
        {
            CategoriaId = categoria.Id,
            PessoaId = pessoa.Id,
            Descricao = "Criada agora",
            Tipo = Transacao.ETipo.Receita,
            Valor = 2000,
            Data = new DateTime(2000, 4, transacaoDay),
        });


        // Act
        List<TotalPorPessoa> totalPorPessoas = await GetTotaisPorPessoaAsync(new()
        {
            ["Periodo.DataFim"] = new DateTime(2000, 4, dataFimDay).ToString("s"),
            ["Pessoa.Id"] = transacao.PessoaId.ToString(),
        });

        // Arrange
        totalPorPessoas.Select(tp => tp.PessoaId).Should().Contain(pessoa.Id);
    }

    private async Task<List<TotalPorPessoa>> GetTotaisPorPessoaAsync(Dictionary<string, string?>? parameters = default)
    {
        parameters ??= new();
        string url = QueryHelpers.AddQueryString($"{BaseEndpoint}/pessoas", parameters);
        int currentPage = 1;
        List<TotalPorPessoa> totaisPorPessoa = [];

        while (true)
        {
            HttpResponseMessage response = await TestClient.GetAsync(url);
            PagedResult<TotalPorPessoa>? result = await response.Content
                   .ReadFromJsonAsync<PagedResult<TotalPorPessoa>>()
                   .ConfigureAwait(false);

            ArgumentNullException.ThrowIfNull(result);
            totaisPorPessoa.AddRange(result.Items);
            if (++currentPage > result.TotalPages)
                break;
        }

        return totaisPorPessoa;
    }

}
