using MinhasFinancas.Domain.Entities;
using MinhasFinancas.Domain.Interfaces;
using Moq;

namespace MinhasFinancas.Tests.Common;

public sealed class PessoaFixture :  BaseFixture
{
    public Pessoa BuildPessoa()
    {
        return new Pessoa
        {
            Id = Guid.NewGuid(),
            Nome = Faker.Name.FullName(),
            DataNascimento = Faker.Date.Past(50, DateTime.UtcNow.AddYears(-12)),
        };
    }

    public List<Pessoa> GeneratePessoas(int count = 3)
    {
        return [.. Enumerable
            .Range(0, count)
            .Select(_ => BuildPessoa())];
    }

    public Pessoa BuildPessoaOfAge(int age = 18)
    {
        Pessoa pessoa = BuildPessoa();
        pessoa.DataNascimento = DateTime.Today.AddYears(-age).AddDays(-1);
        return pessoa;
    }

    public Mock<IPessoaRepository> GetMockedPessoaRepository() => new();
}

[CollectionDefinition(nameof(PessoaFixture))]
public sealed class PessoaCollection : ICollectionFixture<PessoaFixture>;
