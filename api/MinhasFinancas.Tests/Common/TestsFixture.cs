using System;
using System.Collections.Generic;
using System.Text;

namespace MinhasFinancas.Tests.Common;

public sealed class TestsFixture
{
    public PessoaFixture PessoaFixture {  get; } = new PessoaFixture();
    public CategoriaFixture CategoriaFixture {  get; } = new CategoriaFixture();
    public TransacaoFixture TransacaoFixture {  get; } = new TransacaoFixture();
}

[CollectionDefinition(nameof(TestsFixture))]
public sealed class TestsCollection : ICollectionFixture<TestsFixture>;
