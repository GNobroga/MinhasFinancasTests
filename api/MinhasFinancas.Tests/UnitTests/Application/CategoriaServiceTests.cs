using Bogus;
using FluentAssertions;
using MinhasFinancas.Application.DTOs;
using MinhasFinancas.Application.Services;
using MinhasFinancas.Domain.Entities;
using MinhasFinancas.Domain.Interfaces;
using MinhasFinancas.Tests.Common;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MinhasFinancas.Tests.UnitTests.Application;

[Collection(nameof(CategoriaFixture))]
[Trait("UnitTests", "Application - Categoria")]
public sealed class CategoriaServiceTests
{

    private readonly ICategoriaService _sut;
    private readonly CategoriaFixture _fixture;
    private readonly Mock<IUnitOfWork> _mockedUnitOfWork;
    private readonly Mock<ICategoriaRepository> _mockedCategoriaRepository;
    private readonly Faker _faker = new("pt_BR");

    public CategoriaServiceTests(CategoriaFixture fixture)
    {
        ArgumentNullException.ThrowIfNull(fixture);
        _fixture = fixture;
        _mockedUnitOfWork = fixture.GetMockedUnitOfWork();
        _mockedCategoriaRepository = fixture.GetMockedCategoryRepository();
        _mockedUnitOfWork.Setup(uow => uow.Categorias)
                .Returns(_mockedCategoriaRepository.Object);
        _sut = new CategoriaService(_mockedUnitOfWork.Object);
    }

    [Fact]
    public async Task Create()
    {
        // Arrange
        CreateCategoriaDto request = new()
        {
            Descricao = _faker.Lorem.Text(),
            Finalidade = _faker.Random.Enum<Categoria.EFinalidade>(),
        };

        _mockedCategoriaRepository.Setup(cr => cr.AddAsync(It.IsAny<Categoria>()))
               .Returns(Task.FromResult(_fixture.BuildCategoria()));

        // Act
        CategoriaDto result = await _sut.CreateAsync(request);

        // Assert
        result.Id.Should().NotBeEmpty();
        result.Descricao.Should().Be(request.Descricao);
        result.Finalidade.Should().Be(request.Finalidade);
        _mockedUnitOfWork.Verify(uow => uow.SaveChangesAsync(), Times.Once);
    }
}
