import { faker } from '@faker-js/faker';
import { expect, test } from '@playwright/test';
import { TestUtils } from '../utils/testUtils';

type PessoaData = {
  id: string;
  nome: string;
  dataNascimento: string;
  setId(id: string): void;
  update(data: Partial<PessoaData>): void;
};

test.describe.serial('Pessoas', () => {
  const pessoa: PessoaData = {
    id: '',
    nome: faker.person.fullName(),
    dataNascimento: TestUtils.formatDate(faker.date.birthdate()),
    setId(id: string) {
      this.id = id;
    },
    update(data: Partial<PessoaData>) {
      Object.assign(this, data);
    },
  };

  test.beforeEach(async ({ page }) => {
    await page.goto('/pessoas');
    await expect(
      page.getByRole('table', { name: 'Tabela de dados' }),
    ).toBeVisible();
  });

  test('deve criar uma pessoa com sucesso', async ({ page }) => {
    const responsePromise = page.waitForResponse(
      (res) =>
        res.url().includes('/api') &&
        res.url().includes('/pessoas') &&
        res.request().method() === 'POST',
    );

    const addButton = page.getByRole('button', { name: 'Adicionar Pessoa' });
    await addButton.click();
    const modal = page.getByRole('dialog');

    expect(modal).toBeVisible();

    const inputNome = modal.getByPlaceholder('Digite o nome');
    const inputDataNascimento = modal.locator('input[name="dataNascimento"]');

    await inputNome.fill(faker.person.fullName());
    await inputDataNascimento.fill(
      TestUtils.formatDate(faker.date.birthdate()),
    );

    await modal.getByRole('button', { name: 'Salvar' }).click();

    const response = await responsePromise;
    const result = await response.json();
    expect(response.status()).toBe(201);
    pessoa.setId(result.id);
  });

  test('deve editar uma pessoa existente', async ({ page }) => {
    expect(pessoa.id).not.toBeFalsy();

    const responsePromise = page.waitForResponse(
      (res) =>
        res.url().includes('/api') &&
        res.url().includes('/pessoas') &&
        res.request().method() === 'PUT',
    );

    const nextButton = page.getByRole('button', { name: 'Próximo' });
    const editButton = page.getByLabel(`Editar ${pessoa.id}`);

    while (await nextButton.isEnabled()) {
      if (await editButton.isVisible()) break;
      await nextButton.click();
    }

    editButton.click();

    const modal = page.getByRole('dialog');

    await expect(modal).toBeVisible();

    const inputNome = modal.locator('input[id="nome"]');
    const inputDataNascimento = modal.locator('input[name="dataNascimento"]');

    pessoa.update({
      nome: faker.person.fullName(),
      dataNascimento: TestUtils.formatDate(faker.date.birthdate()),
    });

    await inputNome.fill(pessoa.nome);
    await inputDataNascimento.fill(pessoa.dataNascimento);
    await modal.getByRole('button', { name: 'Salvar' }).click();

    const response = await responsePromise;

    expect(response.status()).toBe(204);
  });

  test('deve listar pessoas na tabela', async ({ page }) => {
    const rows = page.locator('tbody > tr');
    await expect(rows.first()).toBeVisible();

    const allRows = await rows.all();

    for (const row of allRows) {
      const cells = row.locator('td');
      expect(await cells.count()).toBe(4);

      const texts = await cells.allTextContents();
      expect(texts[0].trim().length).toBeGreaterThan(0);
      expect(texts[1].trim().length).toBeGreaterThan(0);
      expect(texts[2].trim().length).toBeGreaterThan(0);
      const actionButtons = await cells.last().locator('button').all();
      expect(await actionButtons[0].textContent()).toBe('Editar');
      expect(await actionButtons[1].textContent()).toBe('Deletar');
    }
  });

  test('deve excluir uma pessoa existente', async ({ page }) => {
    expect(pessoa.id).not.toBeFalsy();

    const responsePromise = page.waitForResponse(
      (res) =>
        res.url().includes('/api') &&
        res.url().includes('/pessoas') &&
        res.url().includes(pessoa.id) &&
        res.request().method() === 'DELETE',
    );

    const nextButton = page.getByRole('button', { name: 'Próximo' });
    const deleteButton = page.getByLabel(`Deletar ${pessoa.id}`);
    while (await nextButton.isEnabled()) {
      if (await deleteButton.isVisible()) break;
      await nextButton.click();
    }

    await deleteButton.click();

    const modal = page.getByRole('dialog');
    await modal
      .getByRole('button', { name: 'Confirmar' })
      .click({ force: true });

    const response = await responsePromise;

    expect(response.status()).toBe(204);
  });
});
