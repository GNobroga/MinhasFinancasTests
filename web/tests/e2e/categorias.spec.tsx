import { faker } from '@faker-js/faker';
import { expect, test } from '@playwright/test';

test.describe('Categorias', () => {
  test('deve criar uma categoria', async ({ page }) => {
    await page.goto('/categorias');
    await expect(
      page.getByRole('table', { name: 'Tabela de dados' }),
    ).toBeVisible();

    await page.getByRole('button', { name: 'Adicionar Categoria' }).click();

    const modal = page.getByRole('dialog');

    await expect(modal).toBeVisible();

    const inputDescricao = modal.getByLabel('Descrição');
    const selectFinalidade = modal.getByLabel('Finalidade');

    await inputDescricao.fill(faker.lorem.words(10));
    await selectFinalidade.selectOption({ label: Math.random() > 0.5 ? 'Receita' : 'Despesa' });

    await modal.getByRole('button', { name: 'Salvar' }).click();
  });
});
