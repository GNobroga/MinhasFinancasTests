import { faker } from '@faker-js/faker';
import { expect, Page, test } from '@playwright/test';
import { TestUtils } from '../utils/testUtils';
import type { CategoriaTipo, Pessoa } from '../../src/types/domain';
import { randomUUID } from 'crypto';

const createCategoria = async (page: Page, tipo: CategoriaTipo) => {
  await page.goto('/categorias');

  const responsePromise = page.waitForResponse(
    (res) =>
      res.url().includes('/api') &&
      res.url().includes('/categorias') &&
      res.request().method() === 'POST',
  );

  await expect(
    page.getByRole('table', { name: 'Tabela de dados' }),
  ).toBeVisible();

  await page.getByRole('button', { name: 'Adicionar Categoria' }).click();

  const modal = page.getByRole('dialog');

  await expect(modal).toBeVisible();

  const inputDescricao = modal.getByLabel('Descrição');
  const selectFinalidade = modal.getByLabel('Finalidade');

  await inputDescricao.fill(faker.lorem.words(10));
  await selectFinalidade.selectOption({ label: tipo });

  await modal.getByRole('button', { name: 'Salvar' }).click();

  const response = await responsePromise;
  const categoria = (await response.json()) as {
    id: string;
    descricao: string;
  };
  return categoria;
};

const createPessoa = async (page: Page, { nome, dataNascimento }: Partial<Pessoa> = {}) => {
  await page.goto('/pessoas');

  const responsePromise = page.waitForResponse(
    (res) =>
      res.url().includes('/api') &&
      res.url().includes('/pessoas') &&
      res.request().method() === 'POST',
  );

  await expect(
    page.getByRole('table', { name: 'Tabela de dados' }),
  ).toBeVisible();

  await page.getByRole('button', { name: 'Adicionar Pessoa' }).click();

  const modal = page.getByRole('dialog');

  await expect(modal).toBeVisible();

  const inputNome = modal.getByPlaceholder('Digite o nome');
  const inputDataNascimento = modal.locator('input[name="dataNascimento"]');

  await inputNome.fill(nome ?? faker.person.fullName());
  await inputDataNascimento.fill(TestUtils.formatDate(dataNascimento ?? faker.date.birthdate()));

  await modal.getByRole('button', { name: 'Salvar' }).click();

  const response = await responsePromise;
  const pessoa = (await response.json()) as { id: string; nome: string };
  return pessoa;
};

test.describe('Transações', () => {

  test('deve criar uma transação', async ({ page }) => {
    const responsePromise = page.waitForResponse(
      (res) =>
        res.url().includes('/api') &&
        res.url().includes('/transacoes') &&
        res.request().method() === 'POST',
    );

    const transacaoTipo = Math.random() > 0.5 ? 'Receita' : 'Despesa';
    const categoria = await createCategoria(page, transacaoTipo);
    const pessoa = await createPessoa(page);
    await page.goto('/transacoes');

    await expect(
      page.getByRole('table', { name: 'Tabela de dados' }),
    ).toBeVisible();

    await page.getByRole('button', { name: 'Adicionar Transação' }).click();

    const modal = page.getByRole('dialog');

    await expect(modal).toBeVisible();

    const inputDescricao = modal.getByLabel('Descrição');
    const inputValor = modal.getByLabel('Valor');
    const inputData = modal.getByLabel('Data');
    const selectTipo = modal.getByLabel('Tipo');
    const inputListaPessoas = modal.getByLabel('Lista de pessoas');
    const inputListaCategorias = modal.getByLabel('Lista de categorias');

    await inputDescricao.fill(faker.lorem.words(10));
    await inputValor.fill(Math.round(faker.number.float({ min: 1, max: 1000 })).toString());
    await inputData.fill(TestUtils.formatDate(faker.date.past()));
    await selectTipo.selectOption({ label: transacaoTipo });
    await inputListaPessoas.fill(pessoa.nome, { force: true });

    await inputListaPessoas.getByRole('option', { name: pessoa.nome }).click({ force: true, clickCount: 2 });

    await inputListaCategorias.fill(categoria.descricao, { force: true });
    await inputListaCategorias.getByRole('option', { name: categoria.descricao }).click({ force: true, clickCount: 2,  });

    await modal.getByRole('button', { name: 'Salvar' }).click();

    const response = await responsePromise;

    expect(response.status()).toBe(201);
  });

  test('menor de idade não pode criar transação de despesa', async ({ page }) => {
    const dataNascimento = new Date();
    dataNascimento.setFullYear(dataNascimento.getFullYear() - 10);
    const pessoa = await createPessoa(page, { nome: randomUUID(), dataNascimento });
    const transacaoTipo = Math.random() > 0.5 ? 'Receita' : 'Despesa';
    const categoria = await createCategoria(page, transacaoTipo);

    await page.goto('/transacoes');

    const addButton = page.getByText('Adicionar Transação');

    await addButton.click();

    const modal = page.getByRole('dialog');

    await expect(modal).toBeVisible();


    const inputDescricao = modal.getByLabel('Descrição');
    const inputValor = modal.getByLabel('Valor');
    const inputData = modal.getByLabel('Data');
    const selectTipo = modal.getByLabel('Tipo');
    const inputListaCategorias = modal.getByLabel('Lista de categorias');
    const inputListaPessoas = modal.getByLabel('Lista de pessoas');


    await inputDescricao.fill(faker.lorem.words(10));
    await inputValor.fill(Math.round(faker.number.float({ min: 1, max: 1000 })).toString());
    await inputData.fill(TestUtils.formatDate(faker.date.past()));
    await selectTipo.selectOption({ label: transacaoTipo });

    await inputListaPessoas.fill(pessoa.nome, { force: true });
    await inputListaPessoas.getByRole('option', { name: pessoa.nome }).click({ force: true, clickCount: 2 });

    await inputListaCategorias.fill(categoria.descricao, { force: true });
    await inputListaCategorias.getByRole('option', { name: categoria.descricao }).click({ force: true, clickCount: 2,  });

    await expect(modal.getByText('Menores só podem registrar despesas.')).toBeVisible();

    await modal.getByRole('button', { name: 'Salvar' }).click();
    await expect(modal).toBeVisible();
  });
});
