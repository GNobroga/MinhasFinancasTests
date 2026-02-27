using Bogus;
using FluentAssertions;
using MinhasFinancas.Application.DTOs;
using MinhasFinancas.Application.Services;
using MinhasFinancas.Domain.Entities;
using MinhasFinancas.Domain.Interfaces;
using MinhasFinancas.Tests.Common;
using Moq;
namespace MinhasFinancas.Tests.UnitTests.Application;

[Trait("UnitTests", "Application - Transacao")]
[Collection(nameof(TestsFixture))]
public sealed class TransacaoServiceTests
{
    private readonly Mock<IUnitOfWork> _mockedUnitOfWork;
    private readonly Mock<ITransacaoRepository> _mockedTransacaoRepository;
    private readonly Mock<ICategoriaRepository> _mockedCategoriaRepository;
    private readonly Mock<IPessoaRepository> _mockedPessoaRepository;
    private readonly TransacaoFixture _fixture;
    private readonly CategoriaFixture _categoriaFixture;
    private readonly PessoaFixture _pessoaFixture;
    private readonly Faker _faker = new("pt_BR");
    private readonly ITransacaoService _sut;

    public TransacaoServiceTests(TestsFixture fixture)
    {
        ArgumentNullException.ThrowIfNull(fixture);
        _fixture = fixture.TransacaoFixture;
        _categoriaFixture = fixture.CategoriaFixture;
        _pessoaFixture = fixture.PessoaFixture;
        _mockedUnitOfWork = _fixture.GetMockedUnitOfWork();
        _mockedTransacaoRepository = _fixture.GetMockedTransacaoRepository();
        _mockedCategoriaRepository = _categoriaFixture.GetMockedCategoryRepository();
        _mockedPessoaRepository = _pessoaFixture.GetMockedPessoaRepository();

        _mockedUnitOfWork.Setup(uow => uow.Transacoes)
                .Returns(_mockedTransacaoRepository.Object);

        _mockedUnitOfWork.Setup(u => u.Pessoas)
                         .Returns(_mockedPessoaRepository.Object);

        _mockedUnitOfWork.Setup(u => u.Categorias)
                         .Returns(_mockedCategoriaRepository.Object);

        _sut = new TransacaoService(_mockedUnitOfWork.Object);
    }

    [Fact]
    public async Task Create()
    {
        // Arrange
        Categoria categoria = _categoriaFixture.BuildCategoria();
        Pessoa pessoa = _pessoaFixture.BuildPessoa();

        CreateTransacaoDto request = new()
        {
            Descricao = _faker.Lorem.Text(),
            Valor = _faker.Random.Decimal(1, 100000),
            Tipo = categoria.Finalidade == Categoria.EFinalidade.Despesa
                ? Transacao.ETipo.Despesa : Transacao.ETipo.Receita,
            CategoriaId = categoria.Id,
            PessoaId = pessoa.Id,
            Data = DateTime.UtcNow,
        };

        _mockedCategoriaRepository.Setup(cr => cr.GetByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(categoria);

        _mockedPessoaRepository.Setup(cr => cr.GetByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(pessoa);

        _mockedTransacaoRepository.Setup(tr => tr.AddAsync(It.Is<Transacao>(t =>
            t.Descricao == request.Descricao &&
            t.Valor == request.Valor &&
            t.Tipo == request.Tipo &&
            t.Categoria == categoria &&
            t.Pessoa == pessoa
        )));

        // Act
        TransacaoDto result = await _sut.CreateAsync(request);

        // Assert
        result.Id.Should().NotBeEmpty();
        result.Descricao.Should().Be(request.Descricao);
        result.Tipo.Should().Be(request.Tipo);
        result.CategoriaId.Should().Be(categoria.Id);
        result.PessoaId.Should().Be(pessoa.Id);
        result.PessoaNome.Should().Be(pessoa.Nome);
        result.Data.Should().Be(request.Data);
        _mockedUnitOfWork.Verify(uow => uow.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Create_QuandoCategoriaNaoEncontrada_DeveLancarExcecao()
    {
        // Arrange
        Categoria? categoria = null;
   
        CreateTransacaoDto request = new();

        _mockedCategoriaRepository.Setup(cr => cr.GetByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(categoria);

        // Act
        Func<Task<TransacaoDto>> action = () => _sut.CreateAsync(request);

        // Assert
        await action.Should()
            .ThrowAsync<ArgumentException>();

        _mockedTransacaoRepository
                .Verify(tr => tr.AddAsync(It.IsAny<Transacao>()), Times.Never);
        _mockedUnitOfWork
                .Verify(uow => uow.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task Create_QuandoPessoaNaoEncontrada_DeveLancarExcecao()
    {
        // Arrange
        Pessoa? pessoa = null;

        CreateTransacaoDto request = new();

        _mockedPessoaRepository.Setup(cr => cr.GetByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(pessoa);

        // Act
        Func<Task<TransacaoDto>> action = () => _sut.CreateAsync(request);

        // Assert
        await action.Should()
               .ThrowAsync<ArgumentException>();

        _mockedTransacaoRepository
                .Verify(tr => tr.AddAsync(It.IsAny<Transacao>()), Times.Never);
        _mockedUnitOfWork
                .Verify(uow => uow.SaveChangesAsync(), Times.Never);
    }

}
