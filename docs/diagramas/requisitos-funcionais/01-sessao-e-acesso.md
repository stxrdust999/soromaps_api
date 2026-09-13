# 01. Sessão e acesso

Rotas: `(auth)/login`, `(auth)/register`, `middleware.ts`.
Único fluxo do produto inteiramente sobre dado real, junto com `/admin/users`.

| ID | Requisito | Critério de aceite | RF-TCC | Estado |
|---|---|---|---|---|
| RF-SES-01 | Cadastrar conta com nome de usuário, e-mail e senha | Formulário válido cria registro em `tbUsuario` e a senha só existe como hash | RF-01 | 🟢 |
| RF-SES-02 | Autenticar por nome de usuário e senha | Credencial correta abre `/home`; incorreta devolve mensagem sem sair da tela | RF-01 | 🟢 |
| RF-SES-03 | Manter a sessão por 7 dias | Cookie `session` assinado em HS256, `httpOnly`, `sameSite=lax`, `secure` em produção, `maxAge` de 7 dias | RF-01 | 🟢 |
| RF-SES-04 | Encerrar a sessão | Ação apaga o cookie e a próxima rota protegida cai em `/login` | RF-01 | 🟢 |
| RF-SES-05 | Barrar rota protegida sem sessão | Doze prefixos em `PROTECTED_ROUTES` redirecionam para `/login`; com sessão, `/login` e `/register` redirecionam para `/home` | — | 🟢 |
| RF-SES-06 | Editar os próprios dados de conta | Alterar nome ou e-mail sem reenviar a senha | RF-02 | 🔴 |
| RF-SES-07 | Recuperar senha esquecida | Fluxo de redefinição por e-mail | RF-01 | 🔴 |

## Evidência

| ID | Onde |
|---|---|
| RF-SES-01 | `src/actions/auth.ts:60` (`registerAction`) → `POST /api/users`, BCrypt na API |
| RF-SES-02 | `src/actions/auth.ts:13` (`loginAction`) → `POST /api/auth/login` |
| RF-SES-03 | `src/lib/session.ts` assina com `crypto.subtle`; flags em `src/actions/auth.ts:41-45` |
| RF-SES-04 | `src/actions/auth.ts:105` (`logoutAction`) — não toca a API, é só o cookie |
| RF-SES-05 | `middleware.ts` |

## Notas

**Quem assina não é quem valida.** A API confere a senha e devolve o usuário; o
Next assina o JWT e grava o cookie. É o que deixa o `middleware.ts` decidir
acesso no runtime Edge sem ida à API a cada navegação — decisão de 2026-07-28.

**RF-SES-06 está travado por um detalhe da API:** `PUT /api/users/{id}` re-hasheia
a senha sempre, então qualquer edição exigiria pedir a senha de novo. Por isso
"Editar perfil" e "Gerenciar Conta" no popover da sidebar seguem sem destino.

**A lista de rotas protegidas está escrita duas vezes** — em `PROTECTED_ROUTES`
e no `matcher` do `config`. Rota nova esquecida em um dos dois passa sem guarda,
e nada no build reclama. Ver `RNF-SEG-04`.

**Buraco de segurança conhecido:** o login diferencia "usuário não encontrado"
de "senha incorreta", o que permite enumerar contas. `RNF-SEG-07`.

---

➡️ [02 — Mapa e ponto](./02-mapa-e-ponto.md)
