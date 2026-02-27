using MinhasFinancas.Domain.Entities;
using MinhasFinancas.Domain.Interfaces;
using Moq;

namespace MinhasFinancas.Tests.Common;

public sealed class CategoriaFixture : BaseFixture
{
    public Categoria BuildCategoria()
    {
        return new Categoria
        {
            Id = Guid.NewGuid(),
            Descricao = Faker.Lorem.Word(),
            Finalidade = Faker.Random.Enum<Categoria.EFinalidade>(),  
        };
    }

    public Mock<ICategoriaRepository> GetMockedCategoryRepository() => new();
}


[CollectionDefinition(nameof(CategoriaFixture))]
public sealed class CategoriaCollection : ICollectionFixture<CategoriaFixture>;
