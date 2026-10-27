# 🚦 04. Moderação de Locais

> **Status:** ✅ Concluído · **Depende de:** [03-CatalogoELocais.md](./03-CatalogoELocais.md) · [Índice](./README.md)

Atualmente a API possui `PATCH /api/places/{id}/status` que altera a coluna `status` em `tb_places`, mas `GET /api/places` lista apenas locais `Approved` e não há registro de histórico de decisões, motivo de rejeição/devolução, decisão em lote nem detecção de duplicatas para abastecer `/admin/moderation`.

---

## 🗄️ Banco de Dados (`tb_moderation_decisions`)

- [x] Criar tabela `tb_moderation_decisions` (`ModerationDecision`) — log append-only (somente inserção):
  - `id` (`uuid` PK, `gen_random_uuid()`)
  - `place_id` (`uuid` FK → `tb_places` `ON DELETE CASCADE`, indexado)
  - `moderator_id` (`uuid` FK → `tb_users` `ON DELETE RESTRICT`, indexado)
  - `previous_status` (`varchar(20)` — `PlaceStatus` anterior)
  - `decision` (`varchar(20)` — `'approved'`, `'returned'`, `'rejected'`)
  - `reason` (`varchar(50)` null — código de motivo padronizado; obrigatório quando `decision` for `returned` ou `rejected`)
  - `feedback_note` (`varchar(500)` null — observação enviada ao autor)
  - `is_undo` (`boolean`, default `false` — indica se foi reversão dentro da janela de 24h)
  - `created_at` (`timestamptz`, default `now()`, indexado)

---

## 🔌 API (`/api/admin/moderation`)

| Método | Rota | Acesso | Descrição |
|---|---|---|---|
| `GET` | `/api/admin/moderation/places` | `AdminOnly` | Lista locais filtráveis por `status` (`pending`, `returned`, `rejected`, `approved`), `categoryId`, `page` e `pageSize`, incluindo ficha do autor (total sugerido vs. aprovado), índice de completude da ficha e alerta de possível duplicata (locais próximos num raio de 100m ou nome similar). |
| `POST` | `/api/admin/moderation/places/{placeId:guid}/decisions` | `AdminOnly` | Registra decisão em `tb_moderation_decisions` e atualiza `tb_places.status` na mesma transação. |
| `POST` | `/api/admin/moderation/decisions/batch` | `AdminOnly` | Aplica decisão em lote para múltiplos `placeIds` na mesma transação. |
| `POST` | `/api/admin/moderation/places/{placeId:guid}/undo` | `AdminOnly` | Reverte a última decisão tomada há menos de 24h gravando uma nova linha em `tb_moderation_decisions` (`is_undo = true`) e restaurando `previous_status`. |
| `GET` | `/api/admin/moderation/history` | `AdminOnly` | Lista o histórico paginado de decisões com moderador, local, decisão, motivo e data. |

### Regras de Negócio
- [x] **Controle de concorrência otimista (`409 Conflict`, `code: "moderation.status_conflict"`):** se o `ExpectedCurrentStatus` enviado pelo moderador não bater com o `status` atual do local (outro admin já moderou o item), abortar com `409`.
- [x] **Motivo obrigatório (`400 BadRequest`, `code: "moderation.reason_required"`):** decisões `returned` e `rejected` exigem `reason` preenchido dentro da lista fechada de motivos.
- [x] **Janela de reversão (`409 Conflict`, `code: "moderation.undo_expired"`):** recusar `undo` se a última decisão tiver mais de 24 horas.
- [x] **Reenvio pelo autor:** quando um autor edita (`PATCH /api/places/{id}`) um local que estava com `status == Returned`, o status volta automaticamente para `Pending` para reentrar na fila.

---

## ✅ Verificação

- [x] Aprovar local cria registro em `tb_moderation_decisions` e torna o local visível em `GET /api/places`.
- [x] Rejeitar ou devolver sem `reason` retorna `400 BadRequest`.
- [x] Dois moderadores decidindo o mesmo local simultaneamente: o segundo recebe `409 Conflict`.
- [x] Desfazer decisão dentro de 24h cria nova linha no histórico e devolve o local para a fila `pending`.
