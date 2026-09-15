# ⭐ 05. Contribuição

> **Status:** 💤 · **Depende de:** [03](./03-lugar-e-catalogo.md) · [Índice](./README.md)

O pilar que nunca saiu do papel: avaliação, conversa em torno dela e voto de
"útil". Destrava `/admin/reviews`, a nota do local e metade do `/feed`.

---

## 🗄️ Banco

- [ ] `reviews`: `id`, `place_id`, `user_id`, `rating` `smallint CHECK 1..5`, `body` `varchar(1000)`, `photo_url` null, `status` (`published`/`removed`), `created_at`, `updated_at`; índices `(place_id, status)`, `user_id`
  - **Sem `UNIQUE (place_id, user_id)`**: avaliar o mesmo lugar duas vezes é o sinal "duplicada" que `/admin/reviews` precisa exibir, não erro a bloquear
- [ ] `review_useful_votes`: PK `(review_id, user_id)`, `created_at` — idempotente por construção
- [ ] `comments`: `id`, `review_id`, `user_id`, `reply_to_id` (auto-referência), `body` `varchar(600)`, `status`, `created_at`; índice `(review_id, status)`
- [ ] `content_removals`: `id`, `target_type` (`review`/`comment`/`place`/`profile`), `target_id`, `reason` (`REMOVAL_REASONS`), `moderator_id`, `report_id` null, `created_at` — **tabela única para os dois caminhos de remoção**
- [ ] Remoção é `status = removed` + linha em `content_removals`; nunca `DELETE` (o selo de verificado e a reincidência dependem do registro)
- [ ] Seeder: avaliações geradas por `ReviewGenerator.MatchAverages` + comentários de `src/mocks/admin-reviews.ts`

---

## 🔌 API

| Método | Rota | Acesso | Faz |
|---|---|---|---|
| `GET` | `/api/markers/{id}/reviews?sort=useful\|recent&page=` | anônimo | Só `published`; `usefulCount` e `viewerMarkedUseful` calculados |
| `POST` | `/api/markers/{id}/reviews` | autenticado | Dispara evento de conquista `review` |
| `PUT` / `DELETE` | `/api/reviews/{id}` | autor | Autor edita/apaga a própria (apagar do autor = `removed` sem motivo de moderação) |
| `PUT` | `/api/reviews/{id}/useful` | autenticado | `INSERT ... ON CONFLICT DO NOTHING`; autor não vota na própria (`403`) |
| `DELETE` | `/api/reviews/{id}/useful` | autenticado | |
| `GET` | `/api/reviews/{id}/comments` | anônimo | Com respostas aninhadas um nível |
| `POST` | `/api/reviews/{id}/comments` | autenticado | `replyToId` opcional |
| `GET` | `/api/admin/reviews?signal=&place=&author=&page=` | admin | KPIs + sinais **spam**, **duplicada**, **discrepante** calculados na consulta |
| `POST` | `/api/admin/removals` | admin | **Endpoint único** usado por `/admin/reviews` e `/admin/reports`; aceita lote |

- [ ] Média do local sempre `AVG(rating) WHERE status = 'published'`

---

## 🖥️ Front

- [ ] `/places/[id]`: nota, total, lista ordenada por útil, comentários com resposta, formulário de avaliação
- [ ] `/admin/reviews`: KPIs, sinais, pivô por autor e por local, remoção em lote, linha expansível
- [ ] `RemovalDialog` (`src/components/blocks/`) chama `POST /api/admin/removals` nas duas telas

Mock que sai: `src/mocks/admin-reviews.ts` (e `PlaceCommentMock` de `markers.ts`).

---

## ✅ Verificação

- [ ] Remover uma avaliação altera a média do local
- [ ] Marcar útil duas vezes não duplica o voto
- [ ] Mesmo autor avaliando o mesmo lugar duas vezes: aceito, e aparece como "duplicada" no admin
- [ ] Remoção por `/admin/reviews` e por `/admin/reports` gera linhas iguais em `content_removals`
