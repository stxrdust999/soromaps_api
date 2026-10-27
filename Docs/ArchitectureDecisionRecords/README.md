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
| [0002](0002-ModuloAutenticacaoESessoes.md) | Módulo de Autenticação e Sessões (JWT + Refresh Token) | Accepted | 2026-10-04 | 01 Autenticação |
| [0003](0003-ArquiteturaEmCamadas.md) | Arquitetura em Camadas por Tipo e Padronização de Responsabilidades | Accepted | 2026-10-10 | Refatoração Geral |
| [0004](0004-PipelineDeModeracaoEDuplicatas.md) | Pipeline de Moderação e Detecção de Duplicatas | Accepted | 2026-10-18 | 04 Moderação & Qualidade de Dados |
| [0005](0005-MotorDeGamificacaoEConquistas.md) | Motor de Gamificação e Conquistas do Explorador | Accepted | 2026-10-19 | 05 Gamificação & Engajamento |
| [0006](0006-PautasEditoriaisEFeedSemGrafo.md) | Pautas Editoriais e Feed Urbano Sem Grafo Social | Accepted | 2026-10-20 | 06 Feed de Atividades & Pautas Editoriais (Stories) |
| [0007](0007-IngestaoOsmEAutomacaoCli.md) | Ingestão de Dados OSM e Automação via CLI | Accepted | 2026-10-25 | 07 Ingestão de Dados & Automação CLI |

---

## 🔄 Ciclo de Vida do ADR

- **Proposed:** Proposta sob discussão da equipe.
- **Accepted:** Decisão aprovada e implementada na rodada.
- **Superseded:** Decisão substituída por um ADR posterior.
- **Deprecated:** Decisão descontinuada que não se aplica mais.
- **Rejected:** Proposta avaliada e descartada.
