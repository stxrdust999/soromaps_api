# 🛡️ Requisitos não funcionais — sistema atual

> Trilha: **atual**. Como o Soromaps se comporta hoje, 2026-08-19, lido do
> código. O TCC não levantou requisitos não funcionais formais — estes foram
> derivados das decisões já tomadas no [`/CLAUDE.md`](../../../CLAUDE.md) e do
> que o código de fato faz.

## Páginas

| Arquivo | Categoria | Requisitos |
|---|---|---|
| [01 — Segurança](./01-seguranca.md) | Sessão, autorização, segredo, dependência | `RNF-SEG-*` |
| [02 — Desempenho](./02-desempenho.md) | Rede, render, cache | `RNF-DES-*` |
| [03 — Usabilidade e acessibilidade](./03-usabilidade-e-acessibilidade.md) | Tema, feedback, erro, idioma | `RNF-USA-*` |
| [04 — Manutenibilidade](./04-manutenibilidade.md) | Arquitetura, padrão, teste, docs | `RNF-MAN-*` |
| [05 — Operação e portabilidade](./05-operacao-e-portabilidade.md) | Deploy, ambiente, banco, observabilidade | `RNF-OPE-*` |
| [06 — Dados e IA](./06-dados-e-ia.md) | Integridade do dado, uso de modelo, privacidade | `RNF-DAD-*` |
| [07 — Placar](./07-placar.md) | — | O que está atendido, parcial e violado, em uma página |

---

## Convenção de ID

`RNF-<CATEGORIA>-<NN>`: `SEG`, `DES`, `USA`, `MAN`, `OPE`, `DAD`.
Prefixo `RNF-` nunca colide com o `RF-` dos
[requisitos funcionais](../requisitos-funcionais/README.md), mesmo com a sigla
`DES` aparecendo nos dois (`RF-DES` é Descobrir, `RNF-DES` é desempenho).

## Legenda de estado

| Marca | Significado |
|---|---|
| 🟢 | Atendido, com evidência no código |
| 🟡 | Parcial — vale em parte do sistema, ou vale sem medição |
| 🔴 | **Violado** — o sistema faz o contrário do que o requisito pede |
| ⚪ | Não medido — não há como afirmar nem negar hoje |

O 🔴 aqui é mais forte que o dos requisitos funcionais. Lá significa "não
construído"; aqui significa que existe algo em produção se comportando de
forma contrária ao requisito.

---

## 🧭 Decisões deste documento

### Todo requisito carrega uma métrica, não um adjetivo
**Decisão:** nada de "o sistema deve ser rápido" ou "deve ser seguro". Cada
linha traz o teste que decide se está atendido — número, comando ou
comportamento observável.
**Motivo:** requisito não funcional sem métrica não é verificável, e requisito
não verificável não reprova nada. "Uma requisição de markers por cruzamento de
limiar de zoom" se testa abrindo a aba de rede; "mapa performático" não.

### O que é violado aparece em vermelho, não é omitido
**Decisão:** a API sem autenticação, o hash de senha na resposta e a ausência
de migrations entram como `RNF-*` 🔴, com o mesmo peso dos atendidos.
**Motivo:** é um documento de estado, não de vitrine. Seis dos requisitos de
segurança estão violados **com a API já publicada na internet** — esconder isso
faria o documento contribuir para o problema. O `/CLAUDE.md` já os lista como
backlog; aqui eles ganham critério de aceite, que é o que falta para fechá-los.

### ⚪ existe para não fingir medição
**Decisão:** acessibilidade, Web Vitals e responsividade ficam ⚪ enquanto não
houver auditoria.
**Motivo:** marcar 🟢 porque "usa Radix" ou porque "parece rápido" transforma o
documento em suposição. ⚪ é uma pendência de instrumentação, e é honesta.
