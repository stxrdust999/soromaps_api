# 👤 02. Usuários

> **Status:** 💤 · **Depende de:** [00](./00-fundacao.md) · Anda junto com [01](./01-autenticacao.md) · [Índice](./README.md)

Substitui `tbUsuario` (descartada, [D2](./decisoes.md#d2--reset-limpo-do-banco))
por `users`, com papel e perfil. É o que destrava o gate de `/admin`, a edição
parcial de dados e, adiante, autoria de ponto e contribuição.

---

## 🗄️ Banco

- [ ] Tabela `users`:

| Coluna | Tipo | Nota |
|---|---|---|
| `id` | `integer` PK | |
| `name` | `varchar(80)` **UNIQUE** | identificador de login hoje |
| `email` | `varchar(160)` **UNIQUE** | |
| `password_hash` | `varchar(72)` | BCrypt custo 12 |
| `role` | `varchar(16)` + `CHECK (role IN ('explorer','admin'))` | default `explorer` |
| `avatar_url` | `varchar(500)` null | |
| `bio` | `varchar(280)` null | |
| `neighborhood` | `varchar(60)` null | bairro declarado — ranking por bairro e motivo `perto` do feed |
| `created_at` / `updated_at` | `timestamptz` | |

- [ ] Unicidade case-insensitive em `name` e `email` (índice sobre `lower(...)`), para `Arthur` e `arthur` não coexistirem
- [ ] Sem `score`/`level`: o título do explorador é derivado de `COUNT(user_achievements)` (decisão de 2026-08-12)
- [ ] Seeder: usuários de demonstração a partir de `src/mocks/community.ts`

---

## 🔌 API

| Método | Rota | Acesso | Faz |
|---|---|---|---|
| `POST` | `/api/users` | anônimo | Cadastro; sempre `role = explorer`; `409` em `name`/`email` duplicado |
| `GET` | `/api/users/me` | autenticado | Dados do próprio usuário |
| `PATCH` | `/api/users/me` | autenticado | Atualização parcial; senha opcional, exige a senha atual para trocar |
| `GET` | `/api/admin/users` | admin | Listagem (paginação server-side, compatível com `useTableConfig`) |
| `GET` | `/api/admin/users/{id}` | admin | Detalhe |
| `PATCH` | `/api/admin/users/{id}` | admin | Edita dados e `role` |
| `DELETE` | `/api/admin/users/{id}` | admin | Remove; revoga as sessões |

- [ ] DTO de saída `UserResponse` sem `password_hash`
- [ ] `PATCH` só re-hasheia quando a senha vier (hoje o `PUT` re-hasheia sempre, e por isso o formulário de edição exige senha)
- [ ] Admin não pode rebaixar o próprio papel se for o último admin (`409`)

---

## 🖥️ Front

- [ ] `src/http/users/users.ts` e `src/actions/users.ts` apontam para `/api/admin/users`
- [ ] Formulário de edição em `/admin/users` deixa de exigir senha
- [ ] Coluna/filtro de papel em `/admin/users`
- [ ] `/settings` passa a ter destino (rota e item de navegação ainda não existem — ver `soromaps_web/docs/todo/user/settings.md`)
- [ ] Perfil (`/profile`) lê `avatar_url`, `bio`, `neighborhood` reais

---

## ✅ Verificação

- [ ] Cadastro com e-mail já usado (qualquer caixa): `409` com `code: user.email_taken`
- [ ] `GET /api/admin/users` nunca contém `password_hash`
- [ ] `PATCH /api/users/me` sem senha mantém o hash anterior
- [ ] Rebaixar o último admin: `409`
