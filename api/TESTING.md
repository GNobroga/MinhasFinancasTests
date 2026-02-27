## ✅ Como executar os testes

Este documento descreve como configurar e executar corretamente os testes do projeto MinhasFinancas, bem como os principais problemas identificados durante a execução da suíte de testes.

***

### 📦 Adicionando o projeto de testes à solution

Adicione o projeto **MinhasFinancas.Tests** à solution MinhasFinancas com o comando:

```bash
dotnet sln add MinhasFinancas.Tests
```

*** 

### **Configuração necessária para os testes de integração**

1️⃣ **Ajuste no projeto ``MinhasFinancas.API``**

No arquivo ``Program.cs``, adicione ao final da classe:

```csharp
public partial class Program
```

Esse ajuste é necessário para permitir o uso do ``WebApplicationFactory``, que depende da classe ``Program`` para inicializar corretamente a aplicação durante os testes de integração.

***

2️⃣ **Ajuste no projeto MinhasFinancas.Domain**

No arquivo ``.csproj`` do projeto ``MinhasFinancas.Domain``, adicione a seguinte linha dentro de um ``ItemGroup``:

```xml
<InternalsVisibleTo Include="MinhasFinancas.Tests" />
```

Isso permite que o projeto de testes tenha acesso aos tipos internos definidos no projeto ``Domain``, viabilizando os testes unitários dessas classes.

***

### ▶️ Executando os testes

Após realizar as configurações acima, execute:

```bash
dotnet test
```

Esse comando irá rodar todos os testes unitários e de integração da solution.

***


### 🧪 Testes implementados

Foram implementados testes unitários e de integração para as seguintes classes:

**Controllers**

* ``CategoriasControllerTests``
* ``PessoasControllerTests``
* ``TotaisControllerTests``
* ``TransacoesControllerTests``

**Repositórios**

* ``PessoaRepositoryTests``

**Domínio**

* ``CategoriaTests``
* ``TransacaoTests``

**Services**

* ``TransacaoServiceTests``
* ``PessoaServiceTests``
* ``CategoriaServiceTests``

***

## 🐞 Bugs encontrados durante os testes

### ❌ Bug 1 – Mapeamento incorreto da data da transação

Ao criar uma **Transação**, o campo Data não está sendo corretamente mapeado da entidade para o DTO.

Como consequência, o DTO recebe o valor padrão (``DateTime.MinValue``), ficando inconsistente com o valor real armazenado na entidade.

***

### ❌ Bug 2 – Tratamento incorreto de exceção ao criar transações

No ``TransacaoService``, ao tentar criar uma transação cujo tipo é diferente do tipo/finalidade da categoria, é lançada uma exceção diferente de ``ArgumentException``.

Entretanto, no ``TransacoesController``, apenas exceções do tipo ``ArgumentException`` são tratadas.

Com isso:

* o erro de validação resulta em ``500 InternalServerError``;
* quando o comportamento correto deveria ser retornar ``400 BadRequest``.

### ❌ Bug 3 – Validação de data de nascimento no endpoint de atualização de pessoa

No PessoasController, no endpoint de atualização, quando é enviado um payload contendo uma data de nascimento inválida:

* ``null``
* ``DateTime.MinValue``
* ``DateTime.MaxValue``

o valor é aceito normalmente, sendo persistido como ``DateTime.MinValue``.

Isso permite que a pessoa fique cadastrada com uma data de nascimento inválida e inconsistente.

***

### ❌ Bug 4 – Exclusão de pessoa inexistente retorna status incorreto

No ``PessoasController``, no endpoint de deleção, quando é solicitada a exclusão de uma pessoa que não existe, o endpoint retorna:

```bash
204 NoContent
```
Entretanto, o comportamento esperado é:

```bash
404 NotFound
```
pois o recurso solicitado para exclusão não foi encontrado.

*** 

### 📌 Observações finais

Os testes evidenciam pontos importantes de validação e padronização de erros na API, principalmente:

* consistência no mapeamento entre entidades e DTOs;
*  uso adequado de exceções para erros de validação;
* retorno correto de códigos HTTP para operações inválidas ou recursos inexistentes.

A correção desses pontos melhora a robustez da API e garante um comportamento mais previsível para os consumidores do serviço.