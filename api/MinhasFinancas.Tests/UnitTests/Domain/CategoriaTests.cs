using FluentAssertions;
using MinhasFinancas.Domain.Entities;
using MinhasFinancas.Tests.Common;

namespace MinhasFinancas.Tests.UnitTests.Domain;

[Collection(nameof(CategoriaFixture))]
[Trait("UnitTests", "Domain - Categoria")]
public sealed class CategoriaTests
{
    private readonly CategoriaFixture _fixture;
    public CategoriaTests(CategoriaFixture fixture)
    {
        ArgumentNullException.ThrowIfNull(fixture);
        _fixture = fixture;
    }
    [Theory]
    [InlineData(Transacao.ETipo.Despesa, Categoria.EFinalidade.Despesa)]
    [InlineData(Transacao.ETipo.Receita, Categoria.EFinalidade.Receita)]
    public void PermiteTipo_QuandoFinalidadeForIgualAoTipoDeTransacao_DeveRetornarVerdadeiro(Transacao.ETipo tipo, Categoria.EFinalidade finalidade)
    {
        // Arrange
        Categoria sut = _fixture.BuildCategoria();

        // Act
        sut.Finalidade = finalidade;

        // Assert
        sut.PermiteTipo(tipo).Should().BeTrue();
    }

    [Fact]
    public void PermiteTipo_QuandoFinalidadeForAmbas_DeveRetornarVerdadeiro()
    {
        // Arrange
        Categoria sut = _fixture.BuildCategoria();

        // Act
        sut.Finalidade = Categoria.EFinalidade.Ambas;

        // Assert
        sut.PermiteTipo(Transacao.ETipo.Despesa).Should().BeTrue();
        sut.PermiteTipo(Transacao.ETipo.Receita).Should().BeTrue();
    }

    [Theory]
    [InlineData(Transacao.ETipo.Despesa, Categoria.EFinalidade.Receita)]
    [InlineData(Transacao.ETipo.Receita, Categoria.EFinalidade.Despesa)]
    public void PermiteTipo_QuandoFinalidadeForDiferenteDoTipoDeTransacao_DeveRetornarFalso(Transacao.ETipo tipo, Categoria.EFinalidade finalidade)
    {
        // Arrange
        Categoria sut = _fixture.BuildCategoria();

        // Act
        sut.Finalidade = finalidade;

        // Assert
        sut.PermiteTipo(tipo).Should().BeFalse();
    }


}
