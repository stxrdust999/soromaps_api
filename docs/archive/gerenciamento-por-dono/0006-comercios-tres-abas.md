# Comércios em três abas, uma casca de tabela genérica

> Módulo: admin/businesses · Rota: `/admin/businesses` · Data: 2026-08-09

## O que foi implementado

Tela em `src/app/(app)/admin/businesses/`, portada de um mockup do Claude
Design, sobre `src/mocks/admin-businesses.ts`. **Três abas, porque são três
trabalhos diferentes:** `Pedidos pendentes` (a fila que exige decisão),
`Comércios verificados` (onde se revoga, não se aprova) e `Pontos sem dono`
(lista de prospecção, não fila).

| Peça | Arquivo |
|---|---|
| Fila em memória, sinais de risco, decisões | `_components/use-business-claims.ts` |
| Casca de tabela das três abas | `_components/business-table.tsx` |
| Colunas por aba | `columns-claims.tsx`, `columns-verified.tsx`, `columns-unclaimed.tsx` |
| Selos de risco | `_components/risk-signal-badges.tsx` |
| Painel de decisão | `_components/claim-detail-sheet.tsx` |
| Mini-mapa ponto × CNPJ | `_components/claim-map-preview.tsx` |
| Comparação de conflito | `_components/conflict-dialog.tsx` |
| Recusa e revogação | `_components/decision-dialog.tsx` |
| Convite de reivindicação | `_components/invite-dialog.tsx` |

## Decisões

### Que o escopo original não previa

- **Cinco sinais de risco por pedido**, e são o coração da tela: `conflito`
  (dois pedidos pelo mesmo ponto), `distancia` (CNPJ a mais de 1 km do pin),
  `dono` (o pedido é transferência, não concessão), `naoComercial` (alguém
  reivindicando parque ou mirante) e `novo` (conta sem histórico). Cada um
  responde a uma forma concreta de fraude.
- **Conflito fecha os dois lados de uma vez.** Aprovar um e deixar o outro
  na fila devolveria o ponto ao perdedor no dia seguinte — daí
  `resolveConflict` receber vencedor e perdedor juntos.
- **Terceiro estado "pedir mais evidência"**, ao lado de aprovar e recusar.
  Mesmo raciocínio do "devolver ao autor" da
  [moderação](../../adr/admin/0003-moderacao-mestre-detalhe.md): pedido incompleto não é
  pedido falso.
- **Distância entre o endereço do CNPJ e o pin.** Consulta externa continua
  fora de escopo, mas comparar o que foi declarado com a coordenada do ponto
  é grátis e é o sinal mais forte disponível.
- **Revogar vínculo** na aba de verificados, com motivo próprio — o escopo
  original só cobria conceder, e vínculo concedido por engano precisava de
  saída.
- **"Convidar dono"** nos pontos sem dono, gerando link de reivindicação.
  Não há envio automático: o contato do estabelecimento não existe em lugar
  nenhum.

### De implementação

- **`BusinessTable` é casca genérica.** As três abas mostram entidades
  diferentes, mas a moldura — chips ligados a filtro de coluna, tabela,
  rodapé de paginação — é a mesma. Genérica em vez de três cópias porque o
  que muda é só `columns`, e comportamento novo entra num lugar só.
- **Painel lateral, não página nem mestre-detalhe.** Aqui a listagem é o
  trabalho principal (ao contrário da moderação, onde a fila *é* a tela) e o
  admin precisa despachar vários pedidos sem perder posição na tabela.
- **Recusa e revogação no mesmo diálogo.** Mesma forma — motivo de lista
  fechada, observação livre, aviso opcional — e o que muda entre elas é
  texto.
- **Mini-mapa em SVG com posição ilustrativa.** O pino do CNPJ é jogado longe
  quando a distância passa de 1 km; não é projeção real, e a pergunta
  ("esses dois endereços são o mesmo lugar?") não pede mais que perto ×
  longe.
- **`COMMERCIAL_CATEGORIES` foi para `src/constants/categories.ts`** — é
  conhecimento de produto, não desta tela.

## Pendências conhecidas

- Seleção múltipla na fila existe mas não tem ação em lote
- Miniaturas de evidência não abrem o anexo
- Os contadores de "comércios verificados" e "pontos sem dono" saem do
  tamanho das listas mock, não de um agregado

Sai de 🟡 quando `tbUsuario` tiver `tipoUsuario`/`CNPJ`, `markers` tiver FK de
dono e existir entidade de reivindicação — ver
[docs/todo/admin/businesses.md](./admin-businesses.md).
