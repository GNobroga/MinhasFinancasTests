using MinhasFinancas.Domain.Entities;
using MinhasFinancas.Domain.Interfaces;
using Moq;

namespace MinhasFinancas.Tests.Common;

public sealed class TransacaoFixture : BaseFixture
{
    public Transacao BuildTransacao(Transacao.ETipo? tipo = default)
    {
        return new Transacao
        {
            Id = Guid.NewGuid(),
            Descricao = Faker.Lorem.Text(),
            Valor = Faker.Random.Decimal(),
            Data = DateTime.UtcNow,
            Tipo = tipo ?? Faker.Random.Enum<Transacao.ETipo>(),
        };
    }
    public Mock<ITransacaoRepository> GetMockedTransacaoRepository() => new();
}

[CollectionDefinition(nameof(TransacaoFixture))]
public sealed class TransacaoCollection : ICollectionFixture<TransacaoFixture>;
