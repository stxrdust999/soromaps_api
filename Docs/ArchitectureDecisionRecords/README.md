# Architecture Decision Records (ADRs)

Este diretório contém o registro vivo e imutável de todas as decisões arquiteturais tomadas no desenvolvimento da API do Soromaps.

## 📋 Regra de Imutabilidade das Rodadas

1. **Cada rodada de desenvolvimento / módulo concluído gera um ADR.**
2. **ADR com status `Accepted` é congelado:** após o commit, **nunca mais se edita o conteúdo de um ADR**.
3. **Mudanças de direção:** qualquer alteração posterior de arquitetura ou regra descrita em um ADR já aceito exige a criação de um **novo ADR**, marcando o anterior como `Superseded by ADR-XXXX`.

---

## 🧭 Índice de Decisões

| ADR | Título | Status | Data | Rodada / Módulo |
|---|---|---|---|---|
| [0001](0001-FundacaoEModuloUsuarios.md) | Fundação da API e Módulo de Usuários | Accepted | 2026-09-27 | 00 Fundação & 02 Usuários |

---

## 🔄 Ciclo de Vida do ADR

- **Proposed:** Proposta sob discussão da equipe.
- **Accepted:** Decisão aprovada e implementada na rodada.
- **Superseded:** Decisão substituída por um ADR posterior.
- **Deprecated:** Decisão descontinuada que não se aplica mais.
- **Rejected:** Proposta avaliada e descartada.
