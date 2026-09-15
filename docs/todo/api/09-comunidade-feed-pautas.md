# 🗞️ 09. Comunidade, feed e pautas

> **Status:** 💤 · **Depende de:** [05](./05-contribuicao.md), [06](./06-atividade.md), [07](./07-gamificacao.md) · [Índice](./README.md)

Três telas que são quase só **consulta** sobre o que as fases anteriores
gravam. As únicas tabelas novas são o "ver menos disso" do feed e as pautas.

---

## 🗄️ Banco

- [ ] `feed_mutes`: PK `(user_id, scope, value)`, `scope` `CHECK IN ('neighborhood','category','kind')`, `value` `varchar(40)`, `created_at`
  - Persiste porque o chip removível precisa sobreviver ao F5
- [ ] `stories`:

| Coluna | Nota |
|---|---|
| `id`, `slug` UNIQUE | URL pública `/pautas/[slug]` |
| `kicker` `varchar(24)`, `title` `varchar(90)`, `summary` `varchar(220)` | |
| `body` `jsonb` | array de 3 a 6 parágrafos (mesma régua do `storyDraftSchema`) |
| `photo_url` | |
| `origin` | `ai` / `staff` |
| `model` `varchar(40)` | **obrigatório quando `origin = 'ai'`** (`CHECK`) — pauta de IA aparece rotulada |
| `status` | `draft` / `published` |
| `reviewed_by`, `published_at` | **obrigatórios quando `status = 'published'`** (`CHECK`) |
| `reading_time` `smallint` | minutos |
| `created_at`, `updated_at` | |

- [ ] `story_places`: PK `(story_id, place_id)`, `order` — de 2 a 4 lugares
- [ ] **Sem tabela de feed:** item de feed é consulta; `reason` e `relevance` são calculados
- [ ] Seeder a partir de `src/mocks/stories.ts`

---

## 🔌 API

### Comunidade

| Método | Rota | Acesso | Faz |
|---|---|---|---|
| `GET` | `/api/community/explorers?q=&page=` | autenticado | Busca por nome |
| `GET` | `/api/community/explorers/{id}` | autenticado | Perfil público: contadores, título, selo, últimas avaliações |
| `GET` | `/api/community/ranking?neighborhood=` | autenticado | Ranking por contribuição, com a linha do próprio usuário mesmo fora do topo |

### Feed

| Método | Rota | Acesso | Faz |
|---|---|---|---|
| `GET` | `/api/feed?sort=relevance\|recent&cursor=` | autenticado | Seis tipos (`review`, `activity_burst`, `new_place`, `achievement`, `milestone`, `curation`), cada um com `reason` obrigatório |
| `GET` / `POST` / `DELETE` | `/api/me/feed-mutes` | autenticado | "Ver menos disso" |

Origem dos motivos:

| Motivo | Consulta |
|---|---|
| `nearby` | `places.neighborhood` contra `users.neighborhood` |
| `saved` | `favorites` do usuário |
| `category` | categorias mais frequentes em `visits` do usuário |
| `city` | volume recente em `reviews`/`visits`, sem recorte pessoal |
| `curation` | `stories` publicadas |

- [ ] `activity_burst` é `GROUP BY place_id` numa janela de tempo ("4 pessoas avaliaram o Cabocafé nas últimas 6 horas")
- [ ] `relevance` = decaimento por idade × peso da fonte, calculado na query
- [ ] Paginação por cursor (feed muda enquanto se rola)
- [ ] Discriminador `kind` serializado como string ([D9](./decisoes.md#d9--erro-em-problemdetails-com-code-texto-no-front) / enum como string)

### Pautas

| Método | Rota | Acesso | Faz |
|---|---|---|---|
| `GET` | `/api/stories?page=` | anônimo | Só `published` |
| `GET` | `/api/stories/{slug}` | anônimo | `404` para `draft` |
| `POST` | `/api/admin/stories` | admin | Salva rascunho (o texto do Gemini é gerado **no Next**, a API só persiste) |
| `PUT` | `/api/admin/stories/{id}` | admin | Edição do rascunho |
| `PATCH` | `/api/admin/stories/{id}/publish` | admin | Grava `reviewed_by` (do token) e `published_at` |
| `GET` | `/api/admin/stories?status=draft` | admin | Fila de revisão |

- [ ] `GEMINI_API_KEY` continua só no Next

---

## 🖥️ Front

- [ ] `/community`, `/community/[id]` e ranking sobre a API
- [ ] `/feed` sobre a API, com os chips de "ver menos disso" persistidos
- [ ] Gerador de pauta salva o rascunho em vez de devolver para copiar à mão; fila de revisão em `/admin`
- [ ] `/pautas/[slug]` sobre a API

Mocks que saem: `src/mocks/feed.ts`, `src/mocks/community.ts`, `src/mocks/stories.ts`.

---

## ✅ Verificação

- [ ] Todo item do feed tem `reason`
- [ ] Silenciar um bairro remove seus itens e o chip persiste após recarregar
- [ ] Pauta `draft` responde `404` na rota pública
- [ ] Publicar sem revisor é impossível (o `CHECK` barra no banco)
- [ ] Pauta `ai` sem `model` é rejeitada
