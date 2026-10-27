# 👤 02. Administração de Usuários

> **Status:** ✅ Concluído · **Depende de:** Baseline (ADRs 0001–0003) · [Índice](./README.md)

O `UsersController` atual já provê cadastro (`POST /api/users`), perfil próprio (`GET/PATCH/DELETE /api/users/me`), perfil público (`GET /api/users/{id}`) e listagem paginada para administradores (`GET /api/users`). No entanto, a tela `/admin/users` do front-end permite ao administrador **editar dados/papel** e **excluir** qualquer usuário selecionado na tabela — operações que hoje só existem para o próprio usuário logado (`/me`).

---

## 🔌 API — Endpoints Faltantes em `UsersController`

| Método | Rota | Acesso | Descrição |
|---|---|---|---|
| `PATCH` | `/api/users/{id:guid}` | `AdminOnly` | Atualiza parcialmente nome, e-mail, biografia, bairro, avatar, senha (opcional) e `role` (`explorer` \| `admin`) de um usuário específico. |
| `DELETE` | `/api/users/{id:guid}` | `AdminOnly` | Remove um usuário pelo ID e revoga/remove suas sessões em cascata. |

### Regras de Negócio e DTOs
- [x] Criar `AdminUpdateUserDto` em `DTOs/Users/AdminUpdateUserDto.cs`:
  - Mesmos campos opcionais de `UpdateUserDto` (`Name`, `Email`, `Password`, `AvatarUrl`, `Biography`, `Neighborhood`), acrescido de `UserRole? Role`.
  - Se `Password` vier nulo ou vazio, **manter o `PasswordHash` atual** (o formulário de edição no admin não obriga redefinir senha).
- [x] **Proteção do último administrador (`DomainErrors.User.LastAdmin` → `409 Conflict`, `code: "user.last_admin"`):**
  - Em `PATCH /api/users/{id}`: se o usuário alvo for `Admin`, o novo `Role` for `Explorer` e não houver nenhum outro `Admin` cadastrado em `tb_users`, recusar com `409 Conflict`.
  - Em `DELETE /api/users/{id}`: se o usuário alvo for o único `Admin` do sistema, recusar com `409 Conflict`.
- [x] Revogar sessões (`tb_sessions`) caso o usuário tenha sua conta excluída ou seu papel alterado.

---

## 🔎 Filtros opcionais na listagem (`GET /api/users`)

- [x] Adicionar parâmetros opcionais de consulta em `GET /api/users`:
  - `role` (`UserRole?`): filtrar por `explorer` ou `admin`.
  - `search` (`string?`): busca parcial case-insensitive (`ILIKE`) por `name` ou `email`.

---

## ✅ Verificação

- [x] `PATCH /api/users/{id}` sem campo `password` altera nome/papel mantendo a senha anterior funcionando no login.
- [x] Alterar e-mail para um já utilizado por outro usuário retorna `409 Conflict` com `code: "user.email_taken"`.
- [x] Tentar excluir ou rebaixar o único administrador retorna `409 Conflict` com `code: "user.last_admin"`.
- [x] Explorador comum chamando `PATCH /api/users/{id}` ou `DELETE /api/users/{id}` recebe `403 Forbidden`.
