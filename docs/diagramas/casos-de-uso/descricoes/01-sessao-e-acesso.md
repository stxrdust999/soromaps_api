# 01. Sessão e acesso — descrições

Diagramas em [`baixo-nivel/01-sessao-e-acesso.md`](../baixo-nivel/01-sessao-e-acesso.md).
Requisitos em [`requisitos-funcionais/01-sessao-e-acesso.md`](../../requisitos-funcionais/01-sessao-e-acesso.md).

---

## RF-SES-01 · Cadastrar conta

| Nome do Caso de Uso | Cadastrar conta |
|---|---|
| Caso de Uso Geral | Gerir sessão |
| Ator Principal | Visitante |
| Ator Secundário | API Soromaps |
| Resumo | Cria a conta do explorador com nome de usuário, e-mail e senha, e já o deixa autenticado — o cadastro não passa pela tela de login. |
| Pré-Condição | Nenhum cookie `session` válido no navegador. |
| Pós-Condição | Registro criado em `tbUsuario` com a senha em hash BCrypt, cookie `session` gravado por 7 dias e navegador em `/home`. |
| **Ações do Ator** | **Ações do Sistema** |
| 1. Acessar `/register` | |
| | 2. Exibir o formulário de cadastro |
| 3. Preencher nome de usuário, e-mail e senha | |
| 4. Clicar em "Cadastrar" | |
| | 5. Validar os campos com `createUserSchema` |
| | 6. Enviar os dados para `POST /api/users` |
| | 7. Receber o usuário criado, com a senha já em hash BCrypt |
| | 8. Assinar o JWT HS256 e gravar o cookie `session` |
| | 9. Redirecionar para `/home` |
| | 5a. Se algum campo for inválido, exibir a mensagem no próprio campo e não enviar nada à API |
| | 6a. Se a API recusar, exibir a mensagem devolvida e manter o formulário preenchido |
| | 6b. Se a API não responder, exibir erro de conexão e registrar em `console.error` |
| Restrições / Validação | Senha com no mínimo 6 caracteres · A senha nunca é guardada em texto (🟢 `RNF-SEG-01`) · O cookie é `httpOnly`, `sameSite=lax` e `secure` em produção (🟢 `RNF-SEG-02`) · Nome de usuário e e-mail deveriam ser únicos (🔴 `RNF-SEG-13`) |

> ⚠️ **Lacunas.** Sem `UNIQUE` em `user_name`/`user_email`, o passo 6a nunca
> dispara por duplicidade: cadastrar um nome já existente **cria a segunda
> conta** e torna o login ambíguo. Falta também confirmação de e-mail.

---

## RF-SES-02 · Entrar

| Nome do Caso de Uso | Entrar |
|---|---|
| Caso de Uso Geral | Gerir sessão |
| Ator Principal | Visitante |
| Ator Secundário | API Soromaps |
| Resumo | Autentica o explorador por nome de usuário e senha, e abre a sessão de 7 dias que libera as rotas protegidas. |
| Pré-Condição | Conta existente. Nenhum cookie `session` válido no navegador. |
| Pós-Condição | Cookie `session` assinado em HS256 gravado por 7 dias e navegador em `/home`. |
| **Ações do Ator** | **Ações do Sistema** |
| 1. Acessar `/login` | |
| | 2. Exibir o formulário de acesso |
| 3. Informar nome de usuário e senha | |
| 4. Clicar em "Entrar" | |
| | 5. Validar o formulário com `loginSchema` |
| | 6. Enviar as credenciais para `POST /api/auth/Login` |
| | 7. Receber `id`, `userName` e `userEmail` após a API conferir o hash BCrypt |
| | 8. Assinar o JWT HS256 com expiração de 7 dias |
| | 9. Gravar o cookie `session` e redirecionar para `/home` |
| | 5a. Se o formulário for inválido, exibir a mensagem no campo e não chamar a API |
| | 7a. Se a credencial for recusada, exibir a mensagem devolvida pela API e permanecer em `/login` |
| | 6a. Se a API estiver fora do ar ou `API_URL` faltar, exibir "Erro de conexão com o servidor de autenticação" |
| Restrições / Validação | Usuário não vazio e senha com no mínimo 6 caracteres · Quem confere a senha é a API, quem assina a sessão é o Next (🟢) · `SESSION_SECRET` ausente derruba a aplicação na subida, não em silêncio (🟢 `RNF-OPE-05`) · A resposta não pode revelar se a conta existe (🔴 `RNF-SEG-05`) · Tentativas deveriam ser limitadas por taxa (🔴 `RNF-SEG-06`) |

> ⚠️ **Lacunas.** O passo 7a repassa o texto da API, que distingue "usuário não
> encontrado" de "senha incorreta" — dá para enumerar contas. E falta um passo
> **"ler o papel do usuário"**: a sessão carrega só `id`, `userName` e
> `userEmail`, então `/admin` fica aberto a qualquer sessão (🔴 `RNF-SEG-08`).

---

## RF-SES-04 · Sair

| Nome do Caso de Uso | Sair |
|---|---|
| Caso de Uso Geral | Gerir sessão |
| Ator Principal | Explorador |
| Ator Secundário | — |
| Resumo | Encerra a sessão apagando o cookie. Não há chamada à API: a sessão é emitida e mantida pelo Next. |
| Pré-Condição | Cookie `session` válido. |
| Pós-Condição | Cookie removido; a próxima rota protegida redireciona para `/login`. |
| **Ações do Ator** | **Ações do Sistema** |
| 1. Abrir o popover do usuário na sidebar | |
| 2. Clicar em "Sair" | |
| | 3. Executar `logoutAction` e apagar o cookie `session` |
| | 4. Redirecionar para `/login` |
| Restrições / Validação | A ação roda no servidor, então o cookie `httpOnly` é removido de fato (🟢) · Não há revogação no servidor: um token copiado antes do logout continua válido até expirar (🔴, aceitável enquanto a sessão for stateless) |

---

## RF-SES-05 · Acessar rota protegida

| Nome do Caso de Uso | Acessar rota protegida |
|---|---|
| Caso de Uso Geral | Gerir sessão |
| Ator Principal | Visitante ou Explorador |
| Ator Secundário | — |
| Resumo | Guarda de rota executada no `middleware.ts` antes de qualquer página autenticada renderizar. |
| Pré-Condição | A URL pedida casa com um dos doze prefixos de `PROTECTED_ROUTES`. |
| Pós-Condição | A rota renderiza, ou o navegador é redirecionado para `/login`. |
| **Ações do Ator** | **Ações do Sistema** |
| 1. Navegar para uma rota protegida | |
| | 2. Ler o cookie `session` no runtime Edge |
| | 3. Verificar a assinatura HS256 e a expiração com `crypto.subtle` |
| | 4. Liberar a rota |
| | 3a. Se não houver cookie ou ele for inválido ou expirado, redirecionar para `/login` |
| | 3b. Se a sessão for válida e a rota for `/login` ou `/register`, redirecionar para `/home` |
| Restrições / Validação | A verificação não pode fazer round-trip à API — é por isso que a assinatura usa `crypto.subtle` e não biblioteca de JWT (🟢 `RNF-OPE-17`) · Toda rota autenticada precisa estar na guarda (🟡 `RNF-SEG-04`) · A guarda não distingue papel (🔴 `RNF-SEG-08`) |

> ⚠️ **Lacunas.** A lista de rotas está escrita **duas vezes** — em
> `PROTECTED_ROUTES` e no `matcher` do `config`. Rota nova esquecida em uma das
> duas passa sem guarda, e nada no build reclama.

---

➡️ [02 — Mapa e ponto](./02-mapa-e-ponto.md)
