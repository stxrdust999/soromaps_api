# 07. Placar

Estado dos 114 requisitos não funcionais, em uma página.

## Por categoria

| Categoria | 🟢 | 🟡 | 🔴 | ⚪ | Total |
|---|---|---|---|---|---|
| [Segurança](./01-seguranca.md) | 6 | 3 | **8** | 0 | 17 |
| [Desempenho](./02-desempenho.md) | 8 | 1 | 1 | 1 | 11 |
| [Usabilidade](./03-usabilidade-e-acessibilidade.md) | 12 | 5 | 1 | 2 | 20 |
| [Manutenibilidade](./04-manutenibilidade.md) | 21 | 2 | 2 | 0 | 25 |
| [Operação](./05-operacao-e-portabilidade.md) | 7 | 2 | **10** | 0 | 19 |
| [Dados e IA](./06-dados-e-ia.md) | 13 | 3 | 6 | 0 | 22 |
| **Total** | **67** | **16** | **28** | **3** | **114** |

O desenho do front está sólido — desempenho, manutenibilidade e usabilidade
somam 41 🟢 contra 4 🔴. O buraco é **operação e segurança**, e os dois estão
ligados: a API publicada sem autenticação, sem migrations e sem observabilidade
é a mesma dívida vista de três ângulos.

## O que está violado, por ordem de urgência

### 🔥 Exposição em produção agora

| ID | O que está acontecendo |
|---|---|
| RNF-SEG-07 | Todo endpoint da API responde sem credencial, na internet |
| RNF-SEG-10 | `/api/users` devolve `user_password` nas respostas |
| RNF-SEG-05 | O login diz se a conta existe antes de conferir a senha |
| RNF-SEG-13 | Sem `UNIQUE` em `user_name`/`user_email` — login ambíguo |
| RNF-SEG-08 | Sem papel: qualquer sessão abre `/admin` e exclui qualquer ponto |
| RNF-DAD-11 | Qualquer sessão dispara o Gemini e queima cota paga |
| RNF-SEG-06 | Login sem limite de tentativa |

Os quatro primeiros dependem só da API. `RNF-SEG-08` destrava de uma vez
`RF-ADM-37`, `RF-PTO-06` e `RF-COM-12`.

### ⚠️ Risco de perder ou corromper dado

| ID | O que está acontecendo |
|---|---|
| RNF-OPE-11 | Schema mantido à mão em dois lugares, sem nada que os compare |
| RNF-OPE-13 | Erro em produção morre em `console.error` no navegador do usuário |
| RNF-OPE-15 | Não há como saber se a API está de pé sem abrir o app |
| RNF-SEG-09 | `markers` não tem dono — não dá nem para autorizar por autoria |
| RNF-DAD-16 | O formulário de ponto valida 5 campos que a API descarta |

### 🧱 Fundação que falta

| ID | O que está acontecendo |
|---|---|
| RNF-MAN-18 | Nenhum teste automatizado, nem runner instalado |
| RNF-MAN-19 | Nenhuma verificação antes do merge — não existe `.github/` |
| RNF-OPE-04 | Nenhum `.env.example` versionado, nos dois repositórios |
| RNF-OPE-02 | Deploy da API é publish manual pelo Visual Studio |
| RNF-OPE-08 | Env não é validado no boot |
| RNF-USA-12 | Nenhum `error.tsx` ou `not-found.tsx` no projeto inteiro |
| RNF-DAD-17 | Onze telas publicadas lendo de `src/mocks/` |
| RNF-OPE-12 | `tbUsuario` ao lado de `markers` |
| RNF-DES-05 | `GET /api/markers` devolve a tabela inteira, sempre |
| RNF-OPE-19 | Sem cliente mobile |

### 📋 Conformidade

| ID | O que está acontecendo |
|---|---|
| RNF-DAD-19 | O titular não consegue excluir a própria conta |
| RNF-DAD-20 | Sem política de privacidade nem termo de uso |
| RNF-DAD-10 | Geração de IA sem trilha de auditoria |
| RNF-OPE-14 | Sem log estruturado no servidor |

## O que não está medido

| ID | O que falta |
|---|---|
| RNF-DES-11 | Coleta de Web Vitals em produção |
| RNF-USA-16 | Auditoria de contraste WCAG AA nos dois temas |
| RNF-USA-20 | Verificação de uso em tela pequena — o dispositivo do caso de uso |

Três pendências de **instrumentação**, não de código: nenhuma delas se resolve
escrevendo componente, e nenhuma pode virar 🟢 por opinião.

## Correções de registro pendentes

Achados ao levantar estes requisitos, ainda não refletidos nos documentos de
estado:

1. **`archive/wiki-trilha-projetada/02-requisitos.md`** — a coluna "Hoje" marca RF-11, RF-12 e RF-13 como
   não iniciados. Foi escrita antes de `/discover`, `/feed`, `/community` e
   `/profile`, e RF-13 foi reinterpretado em 2026-08-17.
2. **`wiki/08-deploy.md` e `/CLAUDE.md`** — descrevem o carregamento de markers
   como quebrado em produção por caminho relativo, mas `next.config.ts` tem um
   `rewrites()` encaminhando `/api/markers/:path*` server-side desde
   2026-06-07. Ver `RNF-OPE-06`: precisa ser conferido contra a Vercel antes de
   trabalhar o item 1 do backlog.

Rastreado como `RNF-MAN-25`.
