# 📊 Diagramas e requisitos

> Retrato do **sistema como ele está implementado**, um assunto por subpasta.

Não confundir com a wiki: lá vive a **infraestrutura** do que roda hoje
(arquitetura, banco, endpoints, autenticação, deploy, backlog). Aqui vive o
**comportamento** — o que o sistema faz, para quem, e sob quais restrições.

O material projetado no TCC (13 requisitos de alto nível, casos de uso, DER,
modelo lógico, sequência MVC) saiu da wiki em 20/08/2026 e está em
[`docs/archive/wiki-trilha-projetada/`](../archive/wiki-trilha-projetada/README.md)
— histórico, não fonte de verdade.

> ⚠️ **Esta pasta está no `.gitignore`** (`/docs/diagramas`), por ser entrega
> local de trabalho. Quem clona o repositório não a recebe, e por isso a wiki
> não linka para cá. É a documentação mais atual de requisitos e modelo de
> dados do projeto vivendo fora do controle de versão.

| Pasta | O que descreve | Trilha |
|---|---|---|
| [`casos-de-uso/`](./casos-de-uso/README.md) | Atores e casos de uso do front atual, em Mermaid — alto nível, mais o detalhamento em [`baixo-nivel/`](./casos-de-uso/baixo-nivel/README.md) | Atual |
| [`requisitos-funcionais/`](./requisitos-funcionais/README.md) | O que o sistema faz, por área, com critério de aceite e rastreabilidade até os RF do TCC | Atual |
| [`requisitos-nao-funcionais/`](./requisitos-nao-funcionais/README.md) | Como o sistema se comporta — segurança, desempenho, usabilidade, manutenibilidade, operação, dados e IA | Atual |
| [`modelo-de-dados/`](./modelo-de-dados/README.md) | Estrutura de banco que as telas de hoje exigem — 20 tabelas em DBML, por domínio | Proposto |

As três se cruzam: um caso de uso do diagrama tem um ou mais `RF-*`, e o que
limita esse `RF-*` costuma estar num `RNF-*`.

## Convenção

- **Mermaid é a fonte de verdade** (decisão de 2026-07-28 no `/CLAUDE.md`) —
  PNG só entra em `docs/archive` como registro do traçado original. Única
  exceção: `modelo-de-dados/`, onde a fonte é o `.dbml`, porque tipo, índice e
  constraint não cabem em Mermaid.
- Todo caso de uso desenhado aqui **existe como tela**. O que ainda não foi
  construído vive em [`docs/todo/`](../todo/README.md), não aqui. `modelo-de-dados/`
  é a exceção declarada: descreve o banco que essas telas exigem, não o que
  existe.
- Cada item carrega o estado do dado: **✅/🟢 persistido** (passa pela API) ou
  **🟡 sobre mock** (a tela funciona, o dado é fictício). Nos requisitos não
  funcionais existe ainda **🔴 violado** e **⚪ não medido**.
- Requisito sem critério de aceite verificável não entra.
