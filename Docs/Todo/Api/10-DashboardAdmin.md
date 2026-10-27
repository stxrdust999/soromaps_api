# 📊 10. Dashboard Administrativo

> **Status:** ✅ Concluído · **Depende de:** [04-ModeracaoDeLocais.md](./04-ModeracaoDeLocais.md), [05-AvaliacoesEComentarios.md](./05-AvaliacoesEComentarios.md), [08-DenunciasEFeedback.md](./08-DenunciasEFeedback.md) · [Índice](./README.md)

Abastece a tela `/admin/dashboard` com uma única chamada agregada de leitura sobre as tabelas reais da plataforma.

---

## 🔌 API (`GET /api/admin/stats`)

| Método | Rota | Acesso | Descrição |
|---|---|---|---|
| `GET` | `/api/admin/stats?period=7d\|30d\|90d` | `AdminOnly` | Retorna todos os indicadores operacionais do painel administrativo para a janela selecionada. |

### Estrutura de Resposta (`AdminDashboardStatsDto`)
- [x] **Filas de Atenção (`AttentionQueuesDto`):**
  - `pendingPlacesCount`: total de locais com `status = 'pending'`.
  - `pendingPlacesOver48hCount`: locais `pending` criados há mais de 48h.
  - `openReportsTargetsCount`: total de alvos distintos com denúncias `status = 'open'`.
  - `flaggedReviewsCount`: avaliações com sinais automáticos ativos (`outlier`, `burst` ou `reported`).
- [x] **KPIs com Comparativo de Período (`KpisDto`):**
  - `activePlaces`: total de locais `approved` e variação na janela vs. janela anterior.
  - `newUsers`: novos registros em `tb_users` na janela vs. janela anterior.
  - `newReviews`: novas avaliações em `tb_reviews` na janela vs. janela anterior.
  - `averageModerationTimeHours`: tempo médio entre `tb_places.created_at` e a primeira decisão em `tb_moderation_decisions`.
- [x] **Séries Temporais Diárias (`DailySeriesItemDto[]`):**
  - Série com todos os dias do intervalo (`generate_series` ou preenchimento de dias sem movimento com `0` no serviço) contendo `date`, `newUsers`, `newReviews`, `newPlaces` e `moderationDecisions`.
- [x] **Distribuição por Categoria (`CategoryDistributionItemDto[]`):**
  - Contagem de locais aprovados e avaliações por categoria.
- [x] **Itens Recentes da Fila:**
  - Os 5 locais `pending` mais antigos e os 5 alvos denunciados com mais denunciantes para ação rápida direto da home do admin.

---

## ✅ Verificação

- [x] Dias sem cadastros ou avaliações aparecem na série temporal com valor `0` (gráfico Recharts sem buracos de data).
- [x] Aprovar um local em `/admin/moderation` reduz imediatamente `pendingPlacesCount` em `GET /api/admin/stats`.
