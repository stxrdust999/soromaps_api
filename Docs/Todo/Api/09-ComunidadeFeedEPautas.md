# 📰 09. Comunidade, Feed e Pautas Editoriais

> **Status:** 💤 Pendente · **Depende de:** [03-CatalogoELocais.md](./03-CatalogoELocais.md), [05-AvaliacoesEComentarios.md](./05-AvaliacoesEComentarios.md), [06-AtividadeDoExplorador.md](./06-AtividadeDoExplorador.md), [07-Gamificacao.md](./07-Gamificacao.md) · [Índice](./README.md)

Abastece as telas `/community`, `/community/[id]`, `/feed` e `/pautas/[slug]`.

---

## 🗄️ Banco de Dados

### 1. Pautas Editoriais (`tb_stories` e `tb_story_places`)
- [ ] Criar tabela `tb_stories` (`Story`):
  - `id` (`uuid` PK, `gen_random_uuid()`)
  - `slug` (`varchar(120)` **UNIQUE**)
  - `title` (`varchar(140)`)
  - `subtitle` (`varchar(280)`)
  - `neighborhood` (`varchar(60)` null)
  - `intro_text` (`text`)
  - `closing_text` (`text` null)
  - `origin` (`varchar(20)` — `'ai'` | `'editorial'`)
  - `status` (`varchar(20)` — `'draft'` | `'published'`, default `'draft'`)
  - `author_id` (`uuid` FK → `tb_users`)
  - `published_at` (`timestamptz` null)
  - `created_at` / `updated_at` (`timestamptz`)
- [ ] Criar tabela `tb_story_places` (`StoryPlace`):
  - PK composta `(story_id, place_id)`
  - `sort_order` (`smallint`)
  - `angle_title` (`varchar(120)`)
  - `body_text` (`text`)
  - `local_tip` (`varchar(280)` null)

### 2. Silenciamentos do Feed (`tb_feed_mutes`)
- [ ] Criar tabela `tb_feed_mutes` (`FeedMute`):
  - `id` (`uuid` PK, `gen_random_uuid()`)
  - `user_id` (`uuid` FK → `tb_users` `ON DELETE CASCADE`, indexado)
  - `scope_type` (`varchar(20)` — `'neighborhood'`, `'category'`, `'item_kind'`)
  - `scope_value` (`varchar(60)`)
  - `created_at` (`timestamptz`, default `now()`)
  - Constraint `UNIQUE (user_id, scope_type, scope_value)`.

---

## 🔌 API

### 1. Comunidade (`/api/community`)
| Método | Rota | Acesso | Descrição |
|---|---|---|---|
| `GET` | `/api/community/explorers?neighborhood=&search=&page=` | Anônimo | Ranking e busca de exploradores ordenado por contribuição (`reviewsCount + approvedPlacesCount + visitsCount`), trazendo `achievementsCount`, título derivado e flag `isVerifiedExplorer`. |
| `GET` | `/api/users/{id:guid}` | Anônimo | Expandir o endpoint existente para retornar contadores públicos (`visitsCount`, `reviewsCount`, `approvedPlacesCount`, `achievementsCount`, `isVerifiedExplorer`), últimas avaliações públicas, locais sugeridos aprovados e conquistas desbloqueadas. |

### 2. Feed Baseado em Lugares (`/api/feed`)
| Método | Rota | Acesso | Descrição |
|---|---|---|---|
| `GET` | `/api/feed?reason=&neighborhood=&cursor=` | Autenticado | Monta o feed combinando eventos recentes (novas avaliações, novos locais aprovados, rajadas de movimento agregadas `≥ 3` no mesmo dia, conquistas raras e pautas publicadas), atribuindo o `reason` (`perto`, `salvo`, `categoria`, `cidade`, `curadoria`) e excluindo itens que casem com `tb_feed_mutes` do usuário. |
| `GET` | `/api/feed/mutes` | Autenticado | Lista filtros "ver menos disso" ativos do usuário. |
| `POST` | `/api/feed/mutes` | Autenticado | Adiciona filtro de silenciamento (`scopeType`, `scopeValue`). |
| `DELETE` | `/api/feed/mutes/{id:guid}` | Autenticado | Remove filtro de silenciamento. |

### 3. Pautas (`/api/stories`)
| Método | Rota | Acesso | Descrição |
|---|---|---|---|
| `GET` | `/api/stories` | Anônimo | Lista pautas com `status == 'published'` (Admin pode filtrar `status=draft`). |
| `GET` | `/api/stories/{slug}` | Anônimo | Detalhe completo da pauta com lugares ordenados por `sort_order`. Retorna `404` se `status == 'draft'` e o chamador não for `Admin`. |
| `POST` | `/api/stories` | `AdminOnly` | Cria rascunho de pauta (manual ou gerado via Gemini no front-end). |
| `PATCH` | `/api/stories/{id:guid}` | `AdminOnly` | Atualiza texto/lugares da pauta. |
| `POST` | `/api/stories/{id:guid}/publish` | `AdminOnly` | Valida que todos os `place_id` vinculados estão `Approved` e publica a pauta (`status = 'published'`, `published_at = UtcNow`). |

---

## ✅ Verificação

- [ ] Pauta em `draft` retorna `404` para usuário anônimo ou explorador e `200` para Admin.
- [ ] Silenciar um bairro em `POST /api/feed/mutes` faz `GET /api/feed` omitir cards daquele bairro imediatamente.
- [ ] `GET /api/community/explorers` reflete contadores reais calculados sobre visitas, avaliações e locais aprovados.
