using Bogus;
using MinhasFinancas.Domain.Interfaces;
using Moq;

namespace MinhasFinancas.Tests.Common;

public abstract class BaseFixture
{
    protected Faker Faker { get; } = new("pt_BR");

    public Mock<IUnitOfWork> GetMockedUnitOfWork() => new();
}