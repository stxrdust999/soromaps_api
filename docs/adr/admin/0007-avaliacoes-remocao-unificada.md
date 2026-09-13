# Avaliações: KPIs, sinais formais e a peça compartilhada com Denúncias

> Módulo: admin/reviews · Rota: `/admin/reviews` · Data: 2026-08-15

## O que foi implementado

Tela em `src/app/(app)/admin/reviews/`, desenhada direto no código (sem
passar pelo Claude Design), sobre `src/mocks/admin-reviews.ts` (33
avaliações, 3 já removidas). KPIs no topo + listagem no padrão do projeto.

| Peça | Arquivo |
|---|---|
| Estado, sinais derivados, remoção lógica | `_components/use-reviews.ts` |
| Quatro KPIs | `_components/reviews-stats.tsx` |
| Colunas e ações de linha | `_components/columns.tsx` |
| Linha com painel expansível | `_components/review-row.tsx` |
| Chips de filtro | `_components/toolbar.tsx` |
| Tabela + barra de lote | `_components/reviews-table.tsx` |

## A peça compartilhada, prometida antes da API existir

`docs/todo/admin/reviews.md` já avisava que esta tela e
[Denúncias e feedback](./0005-denuncias-agrupadas-por-alvo.md) removem o
mesmo conteúdo por caminhos diferentes, e que duas implementações
divergiriam no primeiro ajuste de regra. Cumprido:

- `src/constants/content-removal.ts` — `REMOVAL_REASONS` saiu de
  `mocks/admin-reports.ts`. Motivo de remoção é conhecimento de produto, não
  dado fictício, e é o vocabulário que as duas telas precisam dividir.
- `src/components/blocks/removal-dialog.tsx` — promovido de
  `admin/reports/_components/`, com `count` novo para remoção em lote.
  `/admin/reports` continua chamando sem `count`.
- `src/components/blocks/star-rating.tsx` — promovido pelo mesmo motivo.

Quando a Server Action nascer, ela já encontra as duas telas falando igual.

## Sinais: três categorias formais

| Categoria | Como se decide |
|---|---|
| `Suspeita de spam` | flag no registro |
| `Duplicada` | mesmo autor já avaliou o mesmo local |
| `Discrepante` | \|nota − média do local\| ≥ 2,5, com o local tendo ≥ 4 avaliações |

`Duplicada` e `Discrepante` são derivadas do conjunto — só aparecem olhando
todas as avaliações juntas, nunca uma a uma. `spam` é flag porque marcar
exige analisar texto, e fingir que existe classificador seria mentira.

Os limiares moram nomeados em `use-reviews.ts` (`DISCREPANCY_THRESHOLD =
2.5`, `MIN_REVIEWS_FOR_AVERAGE = 4`), com o porquê do número no comentário.

**A rajada de notas máximas não vira selo.** Detectar janela temporal seria a
heurística complicada que o escopo evita; ela aparece pivotando "Ver todas
deste local", que é a ação construída para isso.

## Decisões

- **Ações de pivô são o coração da tela.** "Ver todas deste autor" e "Ver
  todas deste local" custam um `setColumnFilters` e transformam a linha numa
  lente sobre o conjunto — sem elas o padrão anômalo continua invisível.
- **Remoção em lote**, com um motivo só para todas. Achar dez avaliações
  fraudulentas do mesmo dia e remover uma a uma é castigo.
- **Removidas ficam na mesma tabela**, atrás do chip de Status, com quem
  removeu e por quê no painel expandido. Exclusão lógica precisa de vitrine
  auditável, e uma aba separada seria superfície a mais.
- **Linha expansível, não tabela aninhada.** Sub-row do TanStack serve para
  dado da mesma forma em hierarquia; comentário tem forma diferente de
  avaliação, e tabela dentro de tabela traria coluna, ordenação e paginação
  para dois ou três itens. `RowCommon` compartilhado ficou intocado — serve
  seis telas e não deve ganhar expansão por causa de uma.
- **`src/types/review.ts` e `src/validations/review.ts` foram apagados.**
  Eram órfãos da era firebase (`rating`/`content`/`username` em inglês,
  `createdAt?: any`, sem referência a local), sem nenhum importador.

## Fronteira com Denúncias

Esta tela e [Denúncias e feedback](./0005-denuncias-agrupadas-por-alvo.md)
removem a mesma coisa por caminhos diferentes: aqui o admin varre por conta
própria, lá ele reage ao que a comunidade sinalizou. A remoção é uma única
server action, consumida pelas duas.

## Pendências conhecidas

- A variação da nota média é fixa no mock
- A observação do diálogo de remoção não é persistida
- "Abrir o local" aponta para `/places/[id]` com o id do mock, que pode não
  existir no banco

Sai de 🟡 quando `Analise` e `Comentario` existirem — ver
[docs/todo/admin/reviews.md](../../todo/admin/reviews.md).
