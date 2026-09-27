# 🚩 08. Denúncias e feedback

> **Status:** 💤 · **Depende de:** [05](./05-contribuicao.md) (precisa haver conteúdo denunciável) · [Índice](./README.md)

Fila única de denúncias agrupada por alvo, e triagem de feedback sobre o
produto. A remoção em si reusa `content_removals` e `POST /api/admin/removals`
da [fase 05](./05-contribuicao.md).

---

## 🗄️ Banco

- [ ] `reports`:

| Coluna | Nota |
|---|---|
| `id` | |
| `target_type` | `review` / `comment` / `place` / `profile` |
| `target_id` | **polimórfico, sem FK** — a aplicação garante que não fique denúncia órfã |
| `reporter_id` | FK → `users` |
| `reason` | `CHECK IN ('spam','offensive','false','inappropriate','out_of_scope')` |
| `status` | `open` / `resolved` / `archived` |
| `resolved_by`, `resolved_at` | |
| `created_at` | |

  `UNIQUE (target_type, target_id, reporter_id)`: sustenta o selo de denúncia
  coordenada — cinco denúncias no mesmo alvo são cinco pessoas.

- [ ] `feedback`: `id`, `user_id` null (anônimo), `type` (`bug`/`suggestion`/`praise`), `message` `varchar(2000)`, `status` (`new`/`read`/`answered`), `route` e `device` (só bug), `created_at`, `answered_at`
- [ ] Seeder a partir de `src/mocks/admin-reports.ts`

**Custo aceito do alvo polimórfico:** o banco não impõe integridade em
`target_id`. A alternativa (quatro FKs nullable com `CHECK` de exclusividade)
transformaria toda consulta da fila num `COALESCE` de quatro caminhos.

---

## 🔌 API

| Método | Rota | Acesso | Faz |
|---|---|---|---|
| `POST` | `/api/reports` | autenticado | Confere que o alvo existe; denunciar de novo o mesmo alvo → `409` |
| `GET` | `/api/admin/reports?status=&type=&page=` | admin | Agrupado por alvo, com contagem, motivos, selo de coordenada e reincidência do autor |
| `PATCH` | `/api/admin/reports/{targetType}/{targetId}` | admin | Resolve ou arquiva todas as denúncias do alvo |
| `POST` | `/api/feedback` | anônimo ou autenticado | Rate limit por IP |
| `GET` | `/api/admin/feedback?type=&status=` | admin | |
| `PATCH` | `/api/admin/feedback/{id}` | admin | Muda status |

- [ ] Remover o conteúdo a partir da fila chama `POST /api/admin/removals` com `reportId` e resolve as denúncias do alvo na mesma transação

---

## 🖥️ Front

- [ ] Botão de denunciar em avaliação, comentário, ponto e perfil
- [ ] Formulário de feedback (com rota e dispositivo automáticos em bug)
- [ ] `/admin/reports` sobre a API: fila por alvo e aba de triagem

Mock que sai: `src/mocks/admin-reports.ts`.

---

## ✅ Verificação

- [ ] Mesmo usuário denunciando o mesmo alvo duas vezes: `409`
- [ ] Remover pela fila marca o conteúdo `removed`, grava `content_removals` com `report_id` e resolve as denúncias
- [ ] Feedback anônimo aceito; excesso de envios responde `429`
