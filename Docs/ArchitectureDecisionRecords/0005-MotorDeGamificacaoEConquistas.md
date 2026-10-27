# ADR-0005: Motor de Gamificação e Conquistas do Explorador

## Status

Accepted

## Data

2026-10-19

## Rodada / Módulo

05 Gamificação & Engajamento

---

## Contexto

Para incentivar o engajamento contínuo, a descoberta de novos pontos da cidade e a contribuição comunitária (avaliações sinceras, envio de fotos e dicas locais), o Soromaps necessita de uma mecânica de gamificação. Os usuários devem receber distintivos (badges) e desbloquear conquistas conforme exploram a cidade e interagem com a plataforma.

O desafio arquitetural era projetar um motor de avaliação de regras flexível, extensível e desacoplado, que não onerasse as operações transacionais de escrita (como salvar uma visita ou enviar uma avaliação), garantisse estrita idempotência (impedindo que a mesma conquista fosse concedida mais de uma vez) e permitisse criar novas metas de engajamento sem necessidade de deploy contínuo de código.

---

## Decisões Tomadas

1. **Catálogo de Regras Dinâmicas em `tb_achievements`:**
   - Criação da tabela `tb_achievements` contendo as regras e metadados das conquistas: nome, descrição, ícone visual, cor de destaque, tipo de evento disparador (`event_type`), tipo de escopo/alvo (`target_type`), quantidade necessária (`required_quantity`), valor do alvo (`target_value`) e flag de ativação (`is_active`).
   - Suporte a múltiplos tipos de eventos mapeados no enum `AchievementEventType` (`Visit`, `Review`, `PlaceCreated`, `Comment`, `Favorite`, `UsefulReviewVote`).
   - Suporte a granularidade de escopo via `AchievementTargetType` (`Any`, `Category`, `Neighborhood`), permitindo badges gerais (ex: "5 visitas") ou segmentadas (ex: "3 visitas a Cafés", "5 visitas no Centro").

2. **Avaliador Idempotente e Desacoplado (`AchievementEvaluatorService`):**
   - Criação do serviço `AchievementEvaluatorService` com interface `IAchievementEvaluatorService`, responsável por orquestrar o processamento de regras pós-evento de forma idempotente (`EvaluateForUserAsync`).
   - O avaliador recupera apenas as conquistas ativas relacionadas ao evento ocorrido e filtra imediatamente aquelas que o usuário já desbloqueou (`alreadyUnlockedIds`), eliminando processamento redundante e garantindo que nunca ocorram registros duplicados em `tb_user_achievements`.

3. **Cálculo de Progresso Sob Demanda (`CalculateProgressAsync`):**
   - O progresso numérico do usuário em relação a metas incompletas é computado diretamente com base nos registros reais da base (`Visits`, `Reviews`, etc.) quando solicitado pela API, sem depender de contadores redundantes desnormalizados em colunas estáticas.

4. **Isolamento de Falhas e Não Bloqueio das Operações Principais:**
   - A avaliação de conquistas atua de forma complementar aos fluxos de negócio. Falhas eventuais na concessão de badges ou no cálculo de progresso não interrompem nem revertem o fluxo principal de registro da ação do explorador (como salvar uma avaliação ou registrar um check-in).

5. **Persistência de Conquistas Desbloqueadas em `tb_user_achievements`:**
   - Tabela associativa com chave composta `(UserId, AchievementId)` e carimbo temporal `unlocked_at`, viabilizando consultas rápidas do perfil do explorador e exibição no feed de novidades urbanas.

---

## Alternativas Consideradas e Descartadas

- **Hardcoding de Conquistas em Código C# (`switch/case` em Services):** Descartada por engessar a evolução da plataforma, exigindo compilação e deploy da aplicação para cada nova conquista sazonal ou alteração de meta numérica.
- **Tabela de Contadores Desnormalizados por Usuário:** Descartada pelo alto risco de descompasso em cenários concorrentes, inconsistências em caso de soft deletes/remoções de conteúdo e overhead de sincronização contínua.
- **Infraestrutura de Mensageria Pesada (RabbitMQ/Kafka):** Descartada por trazer complexidade operacional excessiva e custo de infraestrutura prematuro para a escala atual da API. O desacoplamento via serviço dedicado no pipeline da aplicação atende com excelência e menor custo de manutenção.

---

## Consequências e Trade-offs

### Positivas (Ganhos)
- Idempotência absoluta: um usuário nunca recebe a mesma conquista duas vezes.
- Flexibilidade total para cadastro de novas conquistas e desafios sazonais diretamente via banco/painel administrativo.
- Resiliência das rotas principais: a experiência de uso não é travada por cálculos de badges.
- Estímulo à retenção do usuário por meio de feedback imediato de progressão na exploração urbana.

### Negativas / Limitações (Custos aceitos)
- Consultas de progresso sob demanda realizam operações de contagem (`CountAsync`) com filtros de categoria e bairro, demandando índices direcionados nas tabelas de atividades conforme o volume de dados cresce.

---

## Referências e Dependências
- [ADR-0001: Fundação da API e Módulo de Usuários](0001-FundacaoEModuloUsuarios.md)
- [ADR-0003: Arquitetura em Camadas por Tipo e Padronização de Responsabilidades](0003-ArquiteturaEmCamadas.md)
