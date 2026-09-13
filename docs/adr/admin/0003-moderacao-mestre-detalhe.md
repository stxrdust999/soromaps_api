# Moderação de pontos em mestre-detalhe, com terceiro estado

> Módulo: admin/moderation · Rota: `/admin/moderation` · Data: 2026-08-09

## O que foi implementado

Tela inteira em `src/app/(app)/admin/moderation/`, portada de um mockup feito
no Claude Design, sobre `src/mocks/admin-moderation.ts`. Layout
**mestre-detalhe** (fila à esquerda, revisão à direita), não tabela + modal:
moderar é trabalho repetitivo e o admin precisa varrer a fila sem perder o
contexto do item.

| Peça | Componente |
|---|---|
| Estado da fila, filtros, seleção, desfazer | `_components/use-moderation-queue.ts` |
| Quatro números do topo | `moderation-stats.tsx` |
| Fila + filtros + ação em lote | `queue-panel.tsx`, `queue-item.tsx` |
| Painel de revisão + barra de decisão | `point-review.tsx` |
| Ficha com completude | `point-fields.tsx` |
| Card do autor | `author-card.tsx` |
| Mini-mapa da vizinhança | `neighborhood-map.tsx` |
| Rejeição com motivo · comparação de duplicata | `reject-dialog.tsx`, `duplicate-dialog.tsx` |
| Aba de auditoria | `history-table.tsx` |

## Decisões

### Que o escopo original não previa

- **Terceiro estado: "devolvido ao autor".** O escopo original só tinha
  aprovar/rejeitar. Ponto incompleto não é ponto ruim, e rejeitar afasta
  contribuidor — a fila ganhou `devolvido` e um contador próprio no topo.
- **Taxa de aprovação do autor** no painel de revisão. É o dado que mais
  acelera a decisão: 24 de 25 aprovados passa rápido, 0 de 3 pede lupa.
- **Motivo de rejeição em lista fechada** (`REJECTION_REASONS`), com
  observação livre opcional. Texto livre não vira métrica de fila nem
  mensagem decente para quem enviou.
- **Vizinhos no mini-mapa.** O escopo pedia "alerta de duplicata"; sem ver o
  que já existe em volta, o alerta não é acionável. Daí também a comparação
  lado a lado com "Mesclar no existente".
- **Barra de completude** contando os 10 campos do formulário, com campo
  vazio aparecendo como "não informado" em vez de sumir da ficha. Amarra com
  o gráfico "Qualidade de dados" do
  [dashboard](./0001-dashboard-sobre-mock.md).
- **Atalhos de teclado** `A`/`D`/`R` e `J`/`K`, com a decisão avançando
  sozinha para o próximo item.

### De implementação

- **Mini-mapa é SVG, não MapLibre.** Aqui não se navega — só se responde "tem
  coisa colada neste pin?". Uma instância de mapa por seleção custaria
  estilo, tiles e ciclo de vida para entregar a mesma resposta. As ruas são
  decorativas; só os pins carregam dado (`vizinhos`, em coordenada
  normalizada).
- **Tokens `--success` e `--warning`** entraram no `globals.css` com
  variantes correspondentes no `Badge`. Só de estado, nunca de marca: o azul
  continua sendo a cor do produto.
- **Desfazer é o `action` do toast do sonner**, sobre um único snapshot da
  lista anterior. A janela de arrependimento é a última decisão, e só.
- **Página fecha altura com `h-dvh`**: o `main` de `(app)/layout.tsx` é
  `min-h-screen overflow-hidden`, então sem altura fechada a lista seria
  cortada em vez de rolar.

## Pendências conhecidas

- Contador ao lado de "Moderação" na sidebar (o mockup tem; depende do
  agregado `GET /api/admin/stats`)
- "Desfazer" da aba Histórico é decorativo
- O modal de rejeição não persiste motivo nem observação em lugar nenhum

Sai de 🟡 quando `markers` tiver `status` e as Server Actions de
aprovar/devolver/rejeitar existirem — ver
[docs/todo/admin/moderation.md](../../todo/admin/moderation.md).
