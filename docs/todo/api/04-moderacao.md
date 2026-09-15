# 🚦 04. Moderação de ponto

> **Status:** 💤 · **Depende de:** [03](./03-lugar-e-catalogo.md) · [Índice](./README.md)

Com `places.status` e `author_id` existindo, a fila de `/admin/moderation`
passa a decidir de verdade. Estado atual mora no ponto; como ele chegou lá mora
em `moderation_decisions`.

---

## 🗄️ Banco

- [ ] `moderation_decisions` — **log só de inserção**:

| Coluna | Nota |
|---|---|
| `id` | |
| `place_id` | FK, índice |
| `moderator_id` | FK → `users` |
| `decision` | `CHECK IN ('approved','returned','rejected')` |
| `reason` `varchar(40)` | lista fechada `REJECTION_REASONS`; null em aprovação |
| `note` `varchar(300)` | |
| `created_at` | índice — "decididos hoje" e tempo médio saem daqui |

- [ ] "Desfazer" em até 24 h é **linha nova**, nunca `DELETE`: o histórico precisa mostrar a reversão
- [ ] Seeder: decisões de demonstração a partir de `src/mocks/admin-moderation.ts`

---

## 🔌 API

| Método | Rota | Acesso | Faz |
|---|---|---|---|
| `GET` | `/api/admin/moderation?status=&category=&page=` | admin | Fila com autor, fotos, completude da ficha e possível duplicata (nome/proximidade) |
| `POST` | `/api/admin/markers/{id}/decisions` | admin | Insere decisão e atualiza `places.status` na mesma transação |
| `POST` | `/api/admin/moderation/decisions` | admin | Decisão em lote (mesma regra, uma transação) |
| `POST` | `/api/admin/markers/{id}/decisions/undo` | admin | Até 24 h; grava decisão inversa |
| `GET` | `/api/admin/moderation/history?page=` | admin | Histórico |

- [ ] Concorrência: decidir ponto cujo `status` já não é o esperado → `409` (dois moderadores na mesma fila)
- [ ] `reason` obrigatório em `rejected`/`returned`, validado contra a lista fechada
- [ ] Aprovação dispara o evento de conquista `create` ([fase 07](./07-gamificacao.md))

---

## 🖥️ Front

- [ ] `/admin/moderation` sobre a API: fila, lote, atalhos, rejeição com motivo, comparação de duplicata, aba de histórico
- [ ] Autor vê status do próprio ponto e pode completar um `returned` e reenviar

Mock que sai: `src/mocks/admin-moderation.ts`.

---

## ✅ Verificação

- [ ] Aprovar muda o ponto para `approved` e ele aparece em `GET /api/markers`
- [ ] Dois admins decidindo o mesmo ponto: o segundo recebe `409`
- [ ] Desfazer cria linha nova; o histórico mostra as duas
- [ ] Desfazer após 24 h: `409`
