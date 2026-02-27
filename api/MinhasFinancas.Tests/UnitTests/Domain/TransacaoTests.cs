using FluentAssertions;
using MinhasFinancas.Domain.Entities;
using MinhasFinancas.Tests.Common;

namespace MinhasFinancas.Tests.UnitTests.Domain;

[Collection(nameof(TestsFixture))]
[Trait("UnitTests", "Domain - Transacao")]
public sealed class TransacaoTests
{
    private readonly TransacaoFixture _fixture;
    private readonly PessoaFixture _pessoaFixture;
    private readonly CategoriaFixture _categoriaFixture;

    public TransacaoTests(TestsFixture fixture)
    {
        ArgumentNullException.ThrowIfNull(fixture);
        _fixture = fixture.TransacaoFixture;
        _pessoaFixture = fixture.PessoaFixture;
        _categoriaFixture = fixture.CategoriaFixture;
    }

    [Fact]
    public void SetPessoa_QuandoPessoaForMenorDeIdadeEForReceita_DeveLancarExcecao()
    {
        // Arrange
        Transacao sut = _fixture.BuildTransacao(Transacao.ETipo.Receita);
        Pessoa pessoa = _pessoaFixture.BuildPessoaOfAge(17);
        string expectedMessage = "Menores de 18 anos não podem registrar receitas.";

        // Act
        Action action = () => sut.Pessoa = pessoa;

        // Assert
        action.Should()
            .Throw<InvalidOperationException>()
            .WithMessage(expectedMessage);
    }

    [Fact]
    public void SetPessoa_QuandoPessoaForMaiorDeIdadeETipoForReceita_NaoDeveLancarExcecao()
    {
        // Arrange
        Transacao sut = _fixture.BuildTransacao(Transacao.ETipo.Receita);
        Pessoa pessoa = _pessoaFixture.BuildPessoaOfAge(18);

        // Act
        Action action = () => sut.Pessoa = pessoa;

        // Assert
        action.Should()
            .NotThrow<InvalidOperationException>();
    }

    [Fact]
    public void SetPessoa_QuandoEntradaForNula_NaoAlteraPessoaId()
    {
        // Arrange
        Transacao sut = _fixture.BuildTransacao(Transacao.ETipo.Receita);
        Pessoa pessoa = _pessoaFixture.BuildPessoaOfAge(18);
        sut.Pessoa = pessoa;

        // Act
        sut.Pessoa = null;
        
        // Assert
        sut.PessoaId.Should().NotBeEmpty();
        sut.PessoaId.Should().Be(pessoa.Id);
    }

    [Theory]
    [MemberData(nameof(GetInvalidFinalidadeAndTipoCombinations))]
    public void SetCategoria_QuandoPermiteTipoForFalso_DeveLancarExcecao(Transacao.ETipo tipo, Categoria.EFinalidade finalidade)
    {
        // Arrange
        Transacao sut = _fixture.BuildTransacao(tipo);
        Categoria categoria = _categoriaFixture.BuildCategoria();
        string expectedMessage = tipo == Transacao.ETipo.Despesa ? "Não é possível registrar despesa em categoria de receita." 
            : "Não é possível registrar receita em categoria de despesa.";
        
        // Act
        categoria.Finalidade = finalidade;
        Action action = () => sut.Categoria = categoria;

        // Assert
        action.Should()
            .Throw<InvalidOperationException>()
            .WithMessage(expectedMessage);
    }

    public static IEnumerable<object[]> GetInvalidFinalidadeAndTipoCombinations() {
        yield return new object[] { Transacao.ETipo.Despesa, Categoria.EFinalidade.Receita };
        yield return new object[] { Transacao.ETipo.Receita, Categoria.EFinalidade.Despesa };
    }

    [Fact]
    public void SetCategoria_QuandoValorNuloForInformado_NaoAlteraCategoriaId()
    {
        // Arrange
        Transacao sut = _fixture.BuildTransacao();
        Categoria categoria = _categoriaFixture.BuildCategoria();
        sut.Tipo = categoria.Finalidade == Categoria.EFinalidade.Despesa 
            ? Transacao.ETipo.Despesa : Transacao.ETipo.Receita;
        sut.Categoria = categoria;

        // Act
        sut.Categoria = null;
        
        // Assert
        sut.CategoriaId.Should().NotBeEmpty();
        sut.CategoriaId.Should().Be(categoria.Id);
    }
        
}
