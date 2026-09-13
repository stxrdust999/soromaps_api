# 01. Segurança

> ⚠️ **A API está publicada na internet sem autenticação.** Seis requisitos
> desta página estão 🔴, e não em rascunho — são o comportamento de produção
> hoje.

## Sessão e credencial

| ID | Requisito | Critério de aceite | Estado |
|---|---|---|---|
| RNF-SEG-01 | Senha nunca é guardada em texto | O banco só tem hash BCrypt; nenhum caminho grava a senha original | 🟢 |
| RNF-SEG-02 | Cookie de sessão inacessível ao script | `httpOnly`, `sameSite=lax`, `secure` em produção, `path=/`, `maxAge` de 7 dias | 🟢 |
| RNF-SEG-03 | Sessão assinada e verificável no Edge | HS256 com `crypto.subtle`, segredo só em `SESSION_SECRET`; sem a variável a aplicação falha ao subir, não degrada em silêncio | 🟢 |
| RNF-SEG-04 | Toda rota autenticada passa pela guarda | Rota nova sem guarda não deveria ser possível | 🟡 |
| RNF-SEG-05 | Login não revela se a conta existe | Credencial errada devolve a mesma mensagem, exista ou não o usuário | 🔴 |
| RNF-SEG-06 | Tentativa de login é limitada por taxa | N tentativas por janela por IP ou conta | 🔴 |

**RNF-SEG-04 é 🟡 por duplicação.** A lista de rotas protegidas está escrita
duas vezes — `PROTECTED_ROUTES` e o `matcher` do `config`, ambos em
`middleware.ts`. Rota nova esquecida em um dos dois passa sem guarda e nada no
build reclama. Uma constante única lida pelos dois fecharia isso.

## Autorização

| ID | Requisito | Critério de aceite | Estado |
|---|---|---|---|
| RNF-SEG-07 | A API exige autenticação | Nenhum endpoint responde sem credencial válida (`AddAuthentication` + `[Authorize]`) | 🔴 |
| RNF-SEG-08 | Existe papel de administrador, checado nos dois lados | `/admin`, editar/excluir ponto e o gerador de pauta exigem papel, no middleware **e** na API | 🔴 |
| RNF-SEG-09 | Recurso tem dono | `markers` referencia quem criou, para autorizar por autoria | 🔴 |

**RNF-SEG-07 é o item 1 do backlog de segurança.** Enquanto ele estiver aberto,
qualquer pessoa com a URL da API faz CRUD completo de usuários e de pontos, sem
sessão nenhuma. `RNF-SEG-08` é inútil sem ele: gate só no front é decoração.

## Exposição de dado e segredo

| ID | Requisito | Critério de aceite | Estado |
|---|---|---|---|
| RNF-SEG-10 | Resposta da API nunca devolve credencial | Nenhum payload contém `user_password`; existe DTO de saída | 🔴 |
| RNF-SEG-11 | Segredo nunca vai para o bundle | `SESSION_SECRET`, `API_URL` e `GEMINI_API_KEY` sem prefixo `NEXT_PUBLIC_`, e ausentes do JavaScript baixado pelo navegador | 🟢 |
| RNF-SEG-12 | Nenhuma chamada de dado sai direto do navegador para a API | Todo tráfego passa pelo servidor Next | 🟡 |
| RNF-SEG-13 | Identificador único onde o domínio exige | `UNIQUE` em `user_name` e `user_email` | 🔴 |
| RNF-SEG-14 | Origem de CORS é configurável por ambiente | Sem host fixo no código | 🔴 |

**RNF-SEG-11 é 🟢 e vale registrar por quê:** `NEXT_PUBLIC_API_URL`
deliberadamente **não** é definida em produção. Com a API sem autenticação,
publicar a URL entregaria o CRUD a quem abrisse o DevTools.

**RNF-SEG-12 é 🟡, não 🔴.** `src/hooks/use-markers.ts` ainda faz `fetch` do
cliente, mas para caminho relativo, e o `rewrites()` de `next.config.ts`
encaminha `/api/markers/:path*` server-side — o navegador não conhece o host da
API. O que falta é tirar a leitura de `NEXT_PUBLIC_API_URL` do hook e mover a
chamada para `src/http`. Ver `RNF-OPE-06`.

**RNF-SEG-13 não é formalidade:** sem `UNIQUE`, duas contas com o mesmo
`user_name` tornam o login ambíguo.

**RNF-SEG-14 hoje é `http://localhost:3000` fixo em `Program.cs`** — a origem de
produção não está liberada, e só não quebra porque o tráfego real passa pelo
servidor.

## Cadeia de dependências

| ID | Requisito | Critério de aceite | Estado |
|---|---|---|---|
| RNF-SEG-15 | Nenhuma vulnerabilidade alta ou crítica em dependência | `npm audit` sem achado alto/crítico | 🟢 |
| RNF-SEG-16 | Nenhuma dependência sem uso no `package.json` | Todo pacote listado tem import em `src/` | 🟢 |
| RNF-SEG-17 | Override fora do range oficial é reconferido a cada bump | O override de `sharp` é revalidado quando o Next sobe de versão | 🟡 |

De 15 avisos para 0: saíram as cinco dependências órfãs (`firebase`, `hono`,
`@hono/node-server`, `leaflet`, `react-leaflet`) — a `firebase` carregava a
única `critical`. O Next subiu para 16.2.12, que corrige um bypass de
middleware/proxy no App Router, justamente onde mora a guarda de rota.

**`npm audit fix --force` está proibido neste repo.** Antes do ajuste, a
"correção" que ele propunha para o `sharp` era instalar `next@14.2.35` — uma
regressão de dois majors.

## Evidência

| ID | Onde |
|---|---|
| RNF-SEG-01 | BCrypt.Net-Next na API; o front nunca vê a senha depois do envio |
| RNF-SEG-02 | `src/actions/auth.ts:41-45` e `:88-92` |
| RNF-SEG-03 | `src/lib/session.ts` — `getSecretKey()` lança sem `SESSION_SECRET` |
| RNF-SEG-04 | `middleware.ts` — `PROTECTED_ROUTES` e `config.matcher` |
| RNF-SEG-11 | `src/lib/gemini.ts`, `src/http/*/*.ts` leem `process.env` sem prefixo público |
| RNF-SEG-12 | `src/hooks/use-markers.ts` + `rewrites()` em `next.config.ts` |
| RNF-SEG-15..17 | `package.json` — `overrides` de `postcss` e `sharp` |

---

➡️ [02 — Desempenho](./02-desempenho.md)
