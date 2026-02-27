using Bogus;
using FluentAssertions;
using MinhasFinancas.Application.DTOs;
using MinhasFinancas.Application.Services;
using MinhasFinancas.Application.Specifications;
using MinhasFinancas.Domain.Entities;
using MinhasFinancas.Domain.Interfaces;
using MinhasFinancas.Domain.ValueObjects;
using MinhasFinancas.Tests.Common;
using Moq;

namespace MinhasFinancas.Tests.UnitTests.Application;

[Collection(nameof(PessoaFixture))]
[Trait("UnitTests", "Application - Pessoa")]
public sealed class PessoaServiceTests
{
    private readonly PessoaFixture _fixture;
    private readonly Mock<IUnitOfWork> _mockedUnitOfWork;

    private readonly Mock<IPessoaRepository> _mockedPessoaRepository;

    private readonly IPessoaService _sut;

    private readonly Faker _faker = new("pt_BR");

    public PessoaServiceTests(PessoaFixture fixture)
    {
        ArgumentNullException.ThrowIfNull(fixture);
        _fixture = fixture;
        _mockedUnitOfWork = _fixture.GetMockedUnitOfWork();
        _mockedPessoaRepository = _fixture.GetMockedPessoaRepository();
        _mockedUnitOfWork.Setup(uow => uow.Pessoas).Returns(_mockedPessoaRepository.Object);
        _sut = new PessoaService(_mockedUnitOfWork.Object);
    }

    [Fact]
    public async Task GetAll()
    {
        // Arrange
        List<Pessoa> pessoas = _fixture.GeneratePessoas(100);
        _mockedPessoaRepository
            .Setup(pr => pr.GetPagedAsync(
                It.IsAny<PagedRequest>(), 
                It.IsAny<LambdaSpecification<Pessoa, PessoaDto>>())
            )
            .ReturnsAsync(new PagedResult<PessoaDto>()
            {
                Items = pessoas.Select(p => new PessoaDto
                {
                    Id = p.Id,
                    Nome = p.Nome,
                    DataNascimento = p.DataNascimento,
                    Idade = p.Idade
                }),
            });
        
        // Act
        PagedResult<PessoaDto> result = await _sut.GetAllAsync();
 
        // Assert
        result.Items.Should().HaveCount(100);
        _mockedPessoaRepository.Verify(pr => 
            pr.GetPagedAsync(
                It.IsAny<PagedRequest>(), 
                It.IsAny<LambdaSpecification<Pessoa, PessoaDto>>()
            ), Times.Once);

    }

    [Fact]
    public async Task GetById_QuandoExistir_DeveRetornarPessoa()
    {
        // Arrange
        Pessoa pessoa = _fixture.BuildPessoa();
        _mockedPessoaRepository.Setup(pr => pr.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(pessoa);

        // Act
        PessoaDto? result = await _sut.GetByIdAsync(pessoa.Id);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(pessoa.Id);
    }

    [Fact]
    public async Task GetById_QuandoNaoExistir_DeveRetornarNulo()
    {
         // Arrange
        Pessoa? pessoa = null;
        _mockedPessoaRepository.Setup(pr => pr.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(pessoa);

        // Act
        PessoaDto? result = await _sut.GetByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Create()
    {
        // Arrange
        CreatePessoaDto request = new()
        {
            Nome = _faker.Person.FullName,
            DataNascimento = _faker.Person.DateOfBirth,
        };

        _mockedPessoaRepository.Setup(pr => pr.AddAsync(It.IsAny<Pessoa>()))
            .Returns(Task.CompletedTask);
        _mockedUnitOfWork.Setup(uow => uow.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        PessoaDto result = await _sut.CreateAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBeEmpty();
        result.Nome.Should().Be(request.Nome);
    }

    [Fact]
    public async Task Update()
    {
        // Arrange
        UpdatePessoaDto request = new()
        {
            Nome = _faker.Person.FullName,
            DataNascimento = DateTime.Today.AddYears(-18),
        };

        Pessoa pessoa = _fixture.BuildPessoa();

        var oldPessoaData = new
        {
            pessoa.Nome,
            pessoa.DataNascimento,
        };

        _mockedPessoaRepository.Setup(pr => pr.GetByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(pessoa);

        _mockedPessoaRepository.Setup(pr => pr.UpdateAsync(pessoa));

        // Act
        await _sut.UpdateAsync(pessoa.Id, request);

        // Assert
        oldPessoaData.Nome.Should().NotBe(pessoa.Nome);
        oldPessoaData.DataNascimento.Should().NotBe(pessoa.DataNascimento);
        pessoa.Nome.Should().Be(request.Nome);
        pessoa.DataNascimento.Should().Be(request.DataNascimento);
        _mockedPessoaRepository.Verify(pr => pr.UpdateAsync(It.IsAny<Pessoa>()), Times.Once);
        _mockedUnitOfWork.Verify(uow => uow.SaveChangesAsync(), Times.Once);

    }

    [Fact]
    public async Task Update_QuandoNaoExistir_DeveLancarExcecao()
    {
        // Arrange
        UpdatePessoaDto request = new()
        {
            Nome = _faker.Person.FullName,
            DataNascimento = DateTime.Today.AddYears(-18),
        };
        string expectedError = "Pessoa não encontrada.";

        _mockedPessoaRepository.Setup(pr => pr.GetByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(default(Pessoa?));

        // Act
        Func<Task> action = () => _sut.UpdateAsync(Guid.NewGuid(), request);

        // Assert
        await action.Should()
                .ThrowAsync<KeyNotFoundException>()
                .WithMessage(expectedError);
    }

    [Fact]
    public async Task Update_QuandoPessoaDtoForNulo_DeveLancarExcecao()
    {
        // Arrange
        UpdatePessoaDto? request = null;

        // Act
        Func<Task> action = () => _sut.UpdateAsync(Guid.NewGuid(), request!);

        // Assert
        await action.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task Delete()
    {
        // Arrange
        Guid id = Guid.NewGuid();

        _mockedPessoaRepository.Setup(pr => pr.DeleteAsync(It.Is<Guid>(val => val == id)));

        // Act
        await _sut.DeleteAsync(id);

        // Assert
        _mockedPessoaRepository.Verify(pr => pr.DeleteAsync(It.IsAny<Guid>()), Times.Once);
        _mockedUnitOfWork.Verify(uow => uow.SaveChangesAsync(), Times.Once);
    }
}
