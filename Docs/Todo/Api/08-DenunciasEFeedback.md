# 🚩 08. Denúncias e Feedback

> **Status:** ✅ Concluído · **Depende de:** [03-CatalogoELocais.md](./03-CatalogoELocais.md), [05-AvaliacoesEComentarios.md](./05-AvaliacoesEComentarios.md) · [Índice](./README.md)

Atende a fila de triagem em `/admin/reports` (denúncias agrupadas por conteúdo alvo) e a tela `/feedback` (envio de bugs, sugestões e elogios à plataforma).

---

## 🗄️ Banco de Dados

### 1. Denúncias (`tb_reports`)
- [x] Criar tabela `tb_reports` (`Report`):
  - `id` (`uuid` PK, `gen_random_uuid()`)
  - `reporter_id` (`uuid` FK → `tb_users` `ON DELETE CASCADE`, indexado)
  - `target_type` (`varchar(20)` — `'place'`, `'review'`, `'comment'`)
  - `target_id` (`uuid` — ID do item denunciado, indexado com `target_type`)
  - `reason` (`varchar(40)` — `'spam'`, `'offensive'`, `'false_info'`, `'closed_place'`, `'wrong_location'`, `'other'`)
  - `details` (`varchar(500)` null)
  - `status` (`varchar(20)` — `'open'`, `'resolved_kept'`, `'resolved_removed'`, default `'open'`)
  - `resolved_by_id` (`uuid` null FK → `tb_users`)
  - `resolution_note` (`varchar(300)` null)
  - `resolved_at` (`timestamptz` null)
  - `created_at` (`timestamptz`, default `now()`)
  - Constraint `UNIQUE (reporter_id, target_type, target_id)` para impedir que o mesmo usuário denuncie o mesmo alvo múltiplas vezes.

### 2. Feedback da Plataforma (`tb_feedback`)
- [x] Criar tabela `tb_feedback` (`Feedback`):
  - `id` (`uuid` PK, `gen_random_uuid()`)
  - `user_id` (`uuid` FK → `tb_users` `ON DELETE CASCADE`)
  - `category` (`varchar(30)` — `'bug'`, `'suggestion'`, `'content'`, `'compliment'`)
  - `origin_path` (`varchar(120)` null — rota de onde o usuário enviou)
  - `message` (`varchar(1000)`)
  - `created_at` (`timestamptz`, default `now()`)

---

## 🔌 API

### Denúncias (`/api/reports` e `/api/admin/reports`)
| Método | Rota | Acesso | Descrição |
|---|---|---|---|
| `POST` | `/api/reports` | Autenticado | Registra denúncia contra local, avaliação ou comentário. Impede auto-denúncia (`409 Conflict`, `code: "report.cannot_report_own"`). |
| `GET` | `/api/admin/reports` | `AdminOnly` | Lista denúncias **agrupadas por `(target_type, target_id)`**, ordenadas pelo número de denunciantes distintos (`reportersCount DESC`), trazendo o snapshot/conteúdo atual do alvo, histórico do autor do conteúdo e lista de motivos alegados. |
| `POST` | `/api/admin/reports/{targetType}/{targetId:guid}/dismiss` | `AdminOnly` | Mantém o conteúdo no ar e fecha todas as denúncias abertas daquele alvo com `status = 'resolved_kept'`. |

> **Nota:** A decisão de **remover** o conteúdo denunciado em `/admin/reports` chama o mesmo serviço de `POST /api/admin/removals` (definido no módulo [05](./05-AvaliacoesEComentarios.md)), que remove o alvo e marca todas as denúncias vinculadas como `resolved_removed`.

### Feedback (`/api/feedback`)
| Método | Rota | Acesso | Descrição |
|---|---|---|---|
| `POST` | `/api/feedback` | Autenticado | Registra feedback do usuário em `tb_feedback`. |
| `GET` | `/api/admin/feedback` | `AdminOnly` | Lista feedbacks recebidos (paginado). |

---

## ✅ Verificação

- [x] Três usuários denunciando a mesma avaliação geram 1 único item agrupado em `GET /api/admin/reports` com `reportersCount = 3`.
- [x] Mesmo usuário tentando denunciar o mesmo item duas vezes recebe `409 Conflict` (`code: "report.already_reported"`).
- [x] Arquivar (`dismiss`) ou remover o conteúdo encerra todas as denúncias abertas daquele `(target_type, target_id)`.
