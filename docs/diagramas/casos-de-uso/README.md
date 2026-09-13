# 🎭 Casos de uso — sistema atual

> Trilha: **atual**. Leitura do que o front do Soromaps faz hoje, em
> 2026-08-19. Para o diagrama de alto nível entregue no TCC, ver
> [`archive/wiki-trilha-projetada/03-casos-de-uso.md`](../../archive/wiki-trilha-projetada/03-casos-de-uso.md).

## Páginas

| Arquivo | O que traz |
|---|---|
| [01 — Visão geral](./01-visao-geral.md) | Atores primários e de sistema, e os pacotes de casos |
| [02 — Explorador](./02-explorador.md) | Sessão, mapa, ponto e descoberta |
| [03 — Explorador social](./03-explorador-social.md) | Feed, comunidade, pauta e perfil |
| [04 — Administrador](./04-administrador.md) | As sete telas de `/admin` |
| [05 — Catálogo](./05-catalogo.md) | Tabela caso × ator × rota × requisito × estado do dado |
| [`baixo-nivel/`](./baixo-nivel/README.md) | Decomposição passo a passo de cada caso, em diagrama |
| [`descricoes/`](./descricoes/README.md) | Os mesmos casos em template expandido: ações do ator × ações do sistema, pré e pós-condição, restrições |

As páginas 01 a 05 ficam no **alto nível** — ator, caso e relação. O passo a
passo desenhado está em [`baixo-nivel/`](./baixo-nivel/README.md) e o mesmo
passo a passo escrito, em [`descricoes/`](./descricoes/README.md). As três
subpastas espelham a mesma divisão por área e os mesmos nomes de arquivo.

Cada caso de uso desta pasta se desdobra em um ou mais requisitos de
[`requisitos-funcionais/`](../requisitos-funcionais/README.md); o que impede um
caso de fechar costuma estar em
[`requisitos-nao-funcionais/`](../requisitos-nao-funcionais/README.md).

---

## Notação adotada

Mermaid **não tem** diagrama de casos de uso nativo, então estes são
`flowchart LR` com uma convenção fixa:

| Forma | Significado |
|---|---|
| `(("👤 Nome"))` | Ator primário |
| `[["🖥️ Nome"]]` | Ator de sistema (secundário) |
| `(["Texto"])` | Caso de uso |
| `---` | Associação ator ↔ caso |
| `-.->\|include\|` | Relação `include` |
| `-.->\|extend\|` | Relação `extend` |
| `-.->\|herda\|` | Generalização entre atores |
| `subgraph` | Pacote / fronteira do subsistema |

É a mesma convenção de `archive/wiki-trilha-projetada/03-casos-de-uso.md`, para os dois diagramas serem
comparáveis lado a lado.

---

## 🧭 Decisões deste diagrama

### Bonequinho de `actor` não atravessa para `flowchart`
**Decisão:** ator é círculo com emoji `👤`, não figura de palito.
**Motivo:** o boneco do `actor` é desenhado pelo renderizador de
`sequenceDiagram` — é a forma daquele tipo de diagrama, não um shape
reutilizável. `flowchart` não tem shape de pessoa em nenhuma versão, nem no
conjunto novo de `@{ shape: ... }` do Mermaid 11, e `actor` dentro de um
`flowchart` é erro de sintaxe, não fallback.
**Alternativas descartadas:**
- **`C4Context` com `Person()`** — desenha um ícone de pessoa de verdade, mas o
  diagrama é experimental, e a semântica é de contexto C4 (`Rel`, `System`,
  `Boundary`), sem `include`/`extend`. Trocaria o vocabulário de casos de uso
  pelo de arquitetura para ganhar um ícone.
- **HTML/`<img>` dentro do rótulo do nó** — depende de `htmlLabels` ligado e
  de `securityLevel` frouxo. O GitHub renderiza Mermaid em modo estrito; o
  rótulo sairia como texto cru.
- **SVG escrito à mão** com palitos UML — o único caminho para o boneco
  canônico, e custa exatamente o que a decisão de 2026-07-28 recusou: diagrama
  que não entra no code review como texto.

### Diagrama por pacote, não um diagrama só
**Decisão:** cinco páginas, uma por fronteira de subsistema.
**Motivo:** o front tem ~60 casos entre explorador e admin. Um `flowchart`
único com todos eles rende um emaranhado que ninguém lê — e o valor do
diagrama de casos de uso é justamente caber num olhar.

### Estado do dado marcado no próprio caso
**Decisão:** cada caso leva ✅ (passa pela API) ou 🟡 (roda sobre mock).
**Motivo:** o produto tem oito telas completas sobre `src/mocks/*` e três
fluxos reais (sessão, usuários, marcadores). Um diagrama que não distinga os
dois mente sobre o estado do sistema — é o mesmo risco que o
[`todo/README.md`](../../todo/README.md) resolve com 🟡.

### Administrador aparece como ator, mesmo sem RBAC
**Decisão:** manter `Administrador` como ator especializado.
**Motivo:** as telas existem e a intenção de papel é real. Mas **não há
checagem de papel em lugar nenhum** — nem coluna no banco, nem no
`middleware.ts`, nem na API: qualquer sessão válida abre `/admin` e edita ou
exclui um ponto em `/places/[id]`. A generalização está desenhada como
`-.->|herda (sem RBAC)|` para o diagrama não prometer uma barreira que não
existe. É item do backlog de segurança no `/CLAUDE.md`.
