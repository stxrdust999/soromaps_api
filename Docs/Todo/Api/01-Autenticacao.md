# 🔐 01. Autenticação

> **Status:** ✅ Entregue (API) · **Depende de:** [00](./00-Fundacao.md) · [Índice](./README.md)
>
> Decisão: [D1 — a API emite, o Next cifra](./Decisoes.md#d1--autenticação-a-api-emite-o-next-cifra)
> (motivo, diagrama de sequência e alternativas descartadas estão lá).

**P1 de segurança:** a API está publicada na internet e hoje **todo endpoint é
público**. Esta fase faz a API validar sozinha quem chama, em vez de confiar em
quem conhece a URL.

---

## 🗄️ Banco

- [x] Tabela `sessions`:

| Coluna | Tipo | Nota |
|---|---|---|
| `id` | `bigint` PK | |
| `user_id` | FK → `users` | índice, delete cascade |
| `token_hash` | `char(64)` UNIQUE | SHA-256 do refresh token; o token em si nunca é gravado |
| `expires_at` | `timestamptz` | 7 dias após emissão |
| `revoked_at` | `timestamptz` null | preenchido em logout, logout-all ou rotação |
| `user_agent` | `varchar(300)` null | para lista de sessões/dispositivos conectados |
| `created_at` | `timestamptz` | default `now()` |

---

## 🔌 API

### Senha (BCrypt existente, ajustado)
- [x] Custo 12 explícito
- [x] Senha limitada a **72 bytes** na validação (o BCrypt ignora o excedente)
- [x] Usuário inexistente: rodar `BCrypt.Verify` contra um hash fixo (`DummyHash`), para o tempo de resposta não denunciar se a conta existe
- [x] Mensagem única de falha (`code: auth.invalid_credentials`), sem distinguir usuário e senha
- [x] `AddRateLimiter` no login (janela fixa de 5 tentativas por minuto por IP)

### Tokens
- [x] Access JWT (15 min) assinado via HMAC-SHA256; claims `sub` (id), `email`, `role`, `jti`
- [x] Refresh: 32 bytes criptograficamente aleatórios (`RandomNumberGenerator`), Base64; só o hash SHA-256 vai para `sessions`
- [x] Chave secreta gerenciada via `dotnet user-secrets` (`Jwt:Key`)

### Endpoints

| Método | Rota | Acesso | Faz |
|---|---|---|---|
| `POST` | `/api/auth/login` | anônimo + rate limit | Confere senha, cria sessão, devolve par de tokens + dados do usuário (DTO) |
| `POST` | `/api/auth/refresh` | anônimo (refresh válido) | Confere hash, revoga o antigo, grava e devolve par novo (rotação) |
| `POST` | `/api/auth/logout` | autenticado | Revoga a sessão do refresh informado |
| `POST` | `/api/auth/logout-all` | autenticado | Revoga todas as sessões do `user_id` ("sair de todos os dispositivos") |

### Autorização
- [x] `AddAuthentication().AddJwtBearer(...)` validando emissor, audiência, assinatura e expiração sem tolerância de clock
- [x] Autenticação exigida por padrão (Fallback Policy); anônimo só onde marcado com `[AllowAnonymous]`
- [x] Policy `AdminOnly` (`role == admin`) registrada para endpoints administrativos

---

## 🖥️ Front

- [ ] Instalar `jose`; reescrever `src/lib/session.ts` sobre ela (substitui o `crypto.subtle` à mão)
- [ ] Cookie `session` passa a ser **JWE** `A256GCM` contendo `{ accessToken, refreshToken }` — `httpOnly`, `secure`, `sameSite=lax`, 7 dias
- [ ] `loginAction`/`registerAction` gravam o JWE com os tokens devolvidos pela API
- [ ] `logoutAction` chama `POST /api/auth/logout` antes de apagar o cookie; nova ação para logout-all
- [ ] `middleware.ts`:
  - decifra o JWE e verifica o access com a **chave pública**;
  - access expirado + refresh presente → chama `/api/auth/refresh`, regrava o cookie na resposta;
  - refresh inválido → redireciona para `/login`;
  - barra `/admin/*` quando `role != admin`
- [ ] `src/lib/fetcher.ts` server-side: decifra o cookie e anexa `Authorization: Bearer`; `src/http/*` e `src/actions/*` passam a usá-lo
- [ ] Remover os Route Handlers órfãos `src/app/api/auth/{login,logout}`
- [ ] Registrar no `CLAUDE.md` do front que a decisão de 2026-07-28 ("sem biblioteca de JWT") foi revertida

### Segredos

| Onde | Chave |
|---|---|
| Azure (API) | `Auth__PrivateKey` |
| Vercel (Next) | chave pública ES256, chave de cifra do JWE (substitui `SESSION_SECRET`) |

---

## ✅ Verificação

- [ ] Sem `Authorization`: endpoint protegido responde `401`
- [ ] Explorador em `/api/admin/*`: `403`
- [ ] Login com usuário inexistente e com senha errada: mesma mensagem, tempos de resposta equivalentes
- [ ] Muitas tentativas de login seguidas: rate limit responde `429`
- [ ] Refresh usado duas vezes: a segunda falha (rotação)
- [ ] Logout-all: todas as sessões do usuário deixam de renovar
- [ ] Access expirado com refresh válido: navegação segue sem voltar ao login
- [ ] Cookie no DevTools é opaco (não decodifica como JWT)
- [ ] Explorador acessando `/admin` no navegador é redirecionado pelo middleware
