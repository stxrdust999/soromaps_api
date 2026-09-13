# 05. Operação e portabilidade

Três provedores: Vercel (front), Azure App Service (API) e Supabase (banco).
Três painéis para configurar e três lugares onde uma variável pode faltar.

## Implantação

| ID | Requisito | Critério de aceite | Estado |
|---|---|---|---|
| RNF-OPE-01 | O front sobe sozinho a cada push | Deploy automático na Vercel, sem passo manual | 🟢 |
| RNF-OPE-02 | A API sobe por pipeline | Publicação automatizada, não pelo Visual Studio de alguém | 🔴 |
| RNF-OPE-03 | O build falha em vez de publicar quebrado | `npm run build` roda `tsc` e falha em erro de tipo | 🟢 |
| RNF-OPE-04 | O ambiente de execução é reproduzível | Toda variável necessária está declarada em um `.env.example` versionado | 🔴 |

**RNF-OPE-04 é 🔴 nos dois repositórios.** A regra `.env*` do `.gitignore`
captura o próprio `.env.example`, então ninguém que clona sabe que precisa de
`API_URL`, `SESSION_SECRET`, `GEMINI_API_KEY` e `GEMINI_MODEL`. A API não tem
`.env.example` nenhum — e tem `bin/` e `obj/` versionados, por falta de
`.gitignore`.

## Configuração e ambiente

| ID | Requisito | Critério de aceite | Estado |
|---|---|---|---|
| RNF-OPE-05 | Variável ausente falha cedo e alto | Faltar `SESSION_SECRET` derruba a aplicação; faltar `GEMINI_API_KEY` é estado esperado e comunicado na tela | 🟡 |
| RNF-OPE-06 | O navegador nunca fala direto com a API | Chamada de dado do cliente é encaminhada pelo servidor Next | 🟡 |
| RNF-OPE-07 | Não há host fixo no código | Origem de CORS e URL de API vêm de configuração | 🔴 |
| RNF-OPE-08 | O ambiente é validado no boot | Um módulo `server-only` valida o env inteiro na subida | 🔴 |

**RNF-OPE-05 é 🟡 por assimetria.** `SESSION_SECRET` lança, e `GEMINI_API_KEY`
degrada com aviso — as duas corretas. Mas `API_URL` ausente só aparece como
`fetch` para `undefined/api/...` em tempo de execução, e `NEXT_PUBLIC_API_URL`
cai em `""` silenciosamente. É o que `RNF-OPE-08` resolveria.

**RNF-OPE-06 merece uma correção de registro.** O `/CLAUDE.md` e
`wiki/08-deploy.md` afirmam que o mapa não carrega marcadores em produção, por
`NEXT_PUBLIC_API_URL` vazia virar caminho relativo e dar 404. Só que
`next.config.ts` tem, desde 2026-06-07, um `rewrites()` encaminhando
`/api/markers/:path*` para `${API_URL}` — que é um proxy server-side, resolve o
caminho relativo e dispensa o CORS. **Precisa ser reconferido contra a Vercel
antes de qualquer trabalho no item 1 do backlog:** se `API_URL` estiver
configurada lá, o sintoma descrito não existe mais, e o que resta é a limpeza
(tirar `NEXT_PUBLIC_API_URL` do hook e levar a chamada para `src/http`), não uma
falha em produção.

## Banco de dados

| ID | Requisito | Critério de aceite | Estado |
|---|---|---|---|
| RNF-OPE-09 | O banco é alcançável de qualquer rede e do provedor | Conexão pelo pooler Supavisor em session mode (IPv4), não pelo host direto IPv6-only | 🟢 |
| RNF-OPE-10 | O ORM não precisa de ajuste para conectar | Session mode é proxy transparente: nada muda no EF Core | 🟢 |
| RNF-OPE-11 | O schema é versionado e reproduzível | EF Core Migrations, uma fonte só | 🔴 |
| RNF-OPE-12 | Nomenclatura de tabela e coluna é consistente | Um padrão só, não `tbUsuario` ao lado de `markers` | 🔴 |

**RNF-OPE-09 tem uma armadilha registrada.** O host direto do Supabase é
IPv6-only desde jan/2024. O sintoma foi a API conectar em rede móvel e nunca em
Wi-Fi — e a saída do Azure App Service é IPv4-only, então de lá o host direto
nunca foi alcançável. Errar o `Username` (`postgres.<project-ref>`) ou o prefixo
`aws-0-`/`aws-1-` devolve `XX000: (ENOTFOUND) tenant/user ... not found`, e não
um erro de senha.

**RNF-OPE-11 é 🔴 com agravante:** o schema é mantido à mão em **dois** lugares
— local e Supabase — e nada compara os dois. Uma coluna criada em um e esquecida
no outro só aparece quando o deploy quebra.

## Observabilidade

| ID | Requisito | Critério de aceite | Estado |
|---|---|---|---|
| RNF-OPE-13 | Erro em produção chega a alguém | Coleta de exceção com alerta, não só `console.error` no navegador do usuário | 🔴 |
| RNF-OPE-14 | Existe log estruturado no servidor | Requisição e falha registradas com correlação | 🔴 |
| RNF-OPE-15 | Há como saber se a API está de pé | Endpoint de health e verificação periódica | 🔴 |

Hoje a única sinalização de falha do carregamento de markers é um
`console.error("Erro ao buscar locais:", ...)` — visível apenas para quem tiver
o DevTools aberto na hora.

## Portabilidade

| ID | Requisito | Critério de aceite | Estado |
|---|---|---|---|
| RNF-OPE-16 | O mapa não depende de conta nem de cota | MapLibre GL com estilos públicos CARTO, sem token | 🟢 |
| RNF-OPE-17 | A guarda de rota roda no runtime Edge | `middleware.ts` decide sem polyfill e sem ida à API | 🟢 |
| RNF-OPE-18 | Trocar de provedor não exige reescrever o produto | Nenhum SDK específico de nuvem no código de domínio | 🟢 |
| RNF-OPE-19 | Existe cliente mobile | App Expo consumindo a mesma API | 🔴 |

**RNF-OPE-16 e RNF-OPE-17 são o motivo de duas decisões de stack:** Mapbox
exigiria chave em variável de ambiente e teria limite mensal; e é
`crypto.subtle`, não uma biblioteca de JWT, que deixa o middleware rodar no
Edge.

## Evidência

| ID | Onde |
|---|---|
| RNF-OPE-03 | `package.json` — `next build` |
| RNF-OPE-05 | `src/lib/session.ts` (lança) e `src/lib/gemini.ts` (envelope `sem-chave`) |
| RNF-OPE-06 | `next.config.ts` — `rewrites()`; `src/hooks/use-markers.ts` |
| RNF-OPE-09, 10 | `wiki/08-deploy.md` — cadeia de conexão pelo pooler |
| RNF-OPE-16 | `src/components/ui/map.tsx` |
| RNF-OPE-17 | `middleware.ts` + `src/lib/session.ts` |

---

➡️ [06 — Dados e IA](./06-dados-e-ia.md)
