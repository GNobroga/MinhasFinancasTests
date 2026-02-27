using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using MinhasFinancas.Application.Services;
using MinhasFinancas.Domain.Entities;
using MinhasFinancas.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinhasFinancas.Tests.IntegrationTests.Services;

public sealed class PessoaRepositoryTests(WebApplicationTestingFactory factory) : BaseIntegrationTest(factory)
{
    public IUnitOfWork UnitOfWork => ScopedServices.GetRequiredService<IUnitOfWork>();

    [Fact]
    public async Task Delete_QuandoExecutadoComSucesso_DeveRemoverTransacoesRelacionadas()
    {
        // Arrange
        IPessoaRepository repository = UnitOfWork.Pessoas;

        var transacao = DbContext.Transacoes
                .Select(t => new { t.Id, t.PessoaId })
                .First();

        // Act
        await repository.DeleteAsync(transacao.PessoaId);

        await UnitOfWork.SaveChangesAsync();

        // Assert
        DbContext.Transacoes.Any(t => t.Id == transacao.Id).Should().BeFalse();
    }
}
