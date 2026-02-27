
# ✅ Como executar os testes

## 2. Instale as dependências de desenvolvimento


```bash
bun install -D @faker-js/faker @playwright/test @vitejs/plugin-react @vitest/browser-playwright vitest vitest-browser-react
```

Essas dependências são necessárias para executar os testes automatizados e integrar o Playwright ao ambiente de testes.

## 3. Instale os navegadores do Playwright

O Playwright precisa que alguns navegadores sejam instalados localmente.
Para isso, execute:

```bash
  bunx playwright install
```

Esse comando baixa automaticamente os navegadores usados durante a execução dos testes.

## 4. Execute os testes

Para rodar os testes no modo visual (interativo), utilize:

```bash
  bunx playwright test --ui
```

O parâmetro --ui abre a interface gráfica do Playwright, permitindo:

* visualizar a execução dos testes em tempo real;

* executar testes individualmente;

* repetir testes com falha facilmente;

* depurar cenários com mais controle.

## 🧪 Testes realizados

Foram implementados **testes de ponta a ponta (E2E)** para validar os principais fluxos das páginas de Categorias, Pessoas e Transações, garantindo que as funcionalidades estejam operando corretamente do ponto de vista do usuário.

Além disso, foi criado um teste unitário para o componente ``TransacoesList``, com o objetivo de verificar o comportamento isolado do componente e identificar possíveis inconsistências na renderização e nos dados exibidos.


# 🐞 Bugs encontrados

### Bug - Endereço da API incorreto

No arquivo `api.ts`, a constante **DEFAULT_API_URL** está definida como:


```js
  const DEFAULT_API_URL = "https://localhost:5000/api/v1.0";
```

Porém, o backend não está disponível nesse endereço.

A API está escutando nos seguintes endpoints:

* HTTPS: `https://localhost:7226`

* HTTP: `http://localhost:5135`

Além disso, as requisições feitas em HTTP são automaticamente redirecionadas para HTTPS pelo middleware UseHttpsRedirection configurado no backend.


***

**Problema**

A aplicação tenta acessar a API em:

```bash
  https://localhost:5000/api/v1.0
```

Esse endereço não corresponde a nenhuma porta configurada no backend, o que causa falha nas requisições.

***

**Resultado esperado**

```bash
  https://localhost:7226/api/v1.0
```

***

**Solução recomendada (via variável de ambiente)**

Criar um arquivo ``.env`` na raiz do projeto e definir a variável:

```bash
  VITE_API_URL=https://localhost:7226/api/v1.0
```

Dessa forma, a URL da API fica configurável e evita a necessidade de alterar código-fonte.

***

**Solução alternativa**

Caso não queira utilizar variáveis de ambiente, basta alterar diretamente o valor da constante no arquivo ``api.ts``:

```bash
  const DEFAULT_API_URL = "https://localhost:7226/api/v1.0";
```

### Bug 2 – Colunas Categoria e Pessoa não exibem dados na tela de Transações

Na página de Transações (``http://localhost:5173/transacoes``), as colunas **Categoria** e **Pessoa** estão sendo exibidas em branco.

Pela modelagem do sistema, uma transação possui relacionamento obrigatório com Categoria e Pessoa, portanto sempre deveria existir alguma informação para ser exibida nessas colunas.

***

**Análise do problema**

No componente ``TransacaoList``, responsável por renderizar a listagem da página, existe a constante ``columns``, que define as colunas da tabela:

```ts
{ key: "categoriaDescricao" as keyof Transacao, header: "Categoria" },
{ key: "pessoaNome" as keyof Transacao, header: "Pessoa" }
```

Entretanto, a interface ``Transacao`` não possui as propriedades ``categoriaDescricao`` e ``pessoaNome``.
O modelo atualmente definido é:

```ts
export interface Transacao {
  id: ID;
  descricao: string;
  valor: number;
  tipo: TipoTransacao;
  categoriaId: ID;
  pessoaId: ID;
  data: Date;
}
```

Ou seja, o componente tenta acessar campos que não existem no objeto de transação, o que faz com que os valores das colunas sejam exibidos como vazios.

***

**Resultado esperado**

As colunas **Categoria** e **Pessoa** devem exibir, respectivamente, a descrição da categoria e o nome da pessoa associadas à transação.

***

**Resultado obtido**

As colunas Categoria e Pessoa são exibidas sem nenhum valor.

Causa raiz

O componente está configurado para ler as propriedades:

* ``categoriaDescricao``

* ``pessoaNome``

porém o objeto ``Transacao`` contém apenas:

* ``categoriaId``

* ``pessoaId``

Essa problemática está acontecendo na função ``mapTransacaoResponse`` porque no backend esses campos estão sendo mapeados corretamente.

✅ Conclusão.

A execução dos testes permitiu validar os principais fluxos da aplicação e identificar problemas relevantes que impactam diretamente o funcionamento e a experiência do usuário.

Durante a análise, foi possível encontrar:

um problema de configuração da URL da API, que impedia a comunicação correta entre frontend e backend;

uma inconsistência no mapeamento dos dados de transações, que fazia com que as colunas de Categoria e Pessoa fossem exibidas em branco na interface.

Os dois bugs possuem causas bem definidas e soluções simples de aplicar, seja por meio de configuração de ambiente ou ajustes no modelo e no mapeamento dos dados.

Com as correções propostas, a aplicação passa a:

consumir corretamente a API;

exibir as informações completas das transações na tela.

Dessa forma, os testes automatizados demonstraram ser fundamentais para detectar falhas que poderiam passar despercebidas no desenvolvimento e contribuem diretamente para a melhoria da qualidade e da confiabilidade do sistema.