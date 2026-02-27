import { describe, expect, test, vi } from 'vitest';
import { render } from 'vitest-browser-react';
import { TransacoesList } from '../../src/pages/TransacoesList';
import type { PagedResult } from '../../src/types/api';
import { TipoTransacao, Transacao } from '../../src/types/domain';
import { ReactNode } from 'react';

type DataTableProps<T> = {
  columns: Column<T>[];
  data: (T & { id: string })[];
};

type Column<T> = {
  key: keyof T;
  header: string;
  align?: string;
  render?: (value: unknown, item?: T) => ReactNode;
};

const mockData: PagedResult<Transacao> = {
  page: 1,
  pageSize: 10,
  total: 1,
  items: [
    {
      id: '1',
      descricao: 'Transação 1',
      valor: 100,
      tipo: TipoTransacao.Despesa,
      categoriaId: '1',
      pessoaId: '1',
      data: new Date(),
    },
  ],
};

vi.mock('@/hooks/useTransacoes', () => ({
  useTransacoes: () => ({
    isLoading: false,
    data: mockData,
  }),
  useCreateTransacao: () => ({}),
}));

vi.mock('@/components/organisms/DataTable', () => ({
  DataTable<T>({ columns, data }: DataTableProps<T>) {
    return (
      <table>
        <thead>
          <tr>
            {columns.map((col) => (
              <th key={col.key as string}>{col.header}</th>
            ))}
          </tr>
        </thead>
        <tbody>
          {data.map((item) => (
            <tr
              data-testId={item.id}
              key={String(item.id ?? JSON.stringify(item))}
            >
              {columns.map((col) => (
                <td data-testId={col.header} key={col.key as string}>
                  {col.render
                    ? col.render(item[col.key], item)
                    : String(item[col.key] ?? '')}
                </td>
              ))}
            </tr>
          ))}
        </tbody>
      </table>
    );
  },
}));

describe('TransacoesList', () => {
  test('deve renderizar', () => {
    render(<TransacoesList />);
  });

  test('deve mapear corretamente os valores das colunas a partir do item', async () => {
    const { getByTestId } = await render(<TransacoesList />);
    const transacoes = mockData.items;
    const columnHeaders = ['Data', 'Descrição', 'Valor', 'Categoria', 'Pessoa'];

    for (const { id } of transacoes) {
      const row = getByTestId(id);
      columnHeaders.forEach((header) => {
        const cell = row.getByTestId(header);
        expect(cell.element().textContent).toBeTruthy();
      });
    }
  });
});
