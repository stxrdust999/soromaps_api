# ADR-0004: Pipeline de Moderação e Detecção de Duplicatas

## Status

Accepted

## Data

2026-10-18

## Rodada / Módulo

04 Moderação & Qualidade de Dados

---

## Contexto

À medida que o Soromaps permite a colaboração pública da comunidade no cadastro de locais, estabelecimentos e atrativos turísticos de Sorocaba, surge a necessidade de assegurar a qualidade e a veracidade das informações cadastradas. Cadastros diretos sem revisão abrem brechas para publicações maliciosas, dados ofensivos, informações incompletas ou duplicatas que fragmentam avaliações e comentários de um mesmo local físico.

Era mandatório estabelecer um fluxo estruturado de moderação com fila de espera assíncrona, ferramentas de avaliação individual e em lote para moderadores, salvaguardas contra enganos operacionais com janela de reversão (undo), alertas automáticos de duplicatas espaciais no momento da análise e rastreabilidade permanente de exclusões de conteúdo.

---

## Decisões Tomadas

1. **Fila Assíncrona e Ciclo de Vida de Moderação:**
   - Todo local cadastrado por usuários comuns inicia com status `Pending`, sendo encaminhado para a fila de moderação acessível exclusivamente por usuários com papel `Admin`.
   - As decisões de moderação abrangem três destinos: `Approved` (local liberado para visualização pública), `Rejected` (local descartado com motivo justificado) e `Returned` (local retornado ao autor para correções com nota explicativa).
   - O moderador pode processar decisões individualmente (`DecideAsync`) ou em lote (`DecideBatchAsync`), otimizando o fluxo operacional sobre filas com múltiplos itens semelhantes.

2. **Registro Histórico e Imutável em `tb_moderation_decisions`:**
   - Criação da tabela `tb_moderation_decisions` com o serviço correspondente `ModerationService`.
   - Cada decisão registra o identificador do local (`PlaceId`), moderador responsável (`ModeratorId`), status anterior (`PreviousStatus`), status concedido (`Decision`), motivo padronizado (`Reason`), observação complementar (`FeedbackNote`) e indicador se a operação foi um desfazimento (`IsUndo`).

3. **Janela de Reversão de 24 Horas (`Undo` Operacional):**
   - O moderador possui uma janela de tolerância de 24 horas (`UndoAsync`) para revogar uma decisão incorreta tomada sobre um local.
   - A reversão não remove o histórico anterior; em vez disso, insere um novo registro com `IsUndo = true`, restaura o status prévio do local e garante transparência e auditabilidade contínua. Após 24 horas, a reversão é bloqueada pelo sistema.

4. **Detecção Preventiva de Duplicatas Geográficas e Nominais via Haversine:**
   - Implementada verificação de proximidade geográfica utilizando a fórmula Haversine em memória no `ModerationService`.
   - Locais pendentes na fila são confrontados com estabelecimentos já cadastrados no raio de proximidade (ex: 50 metros) e similaridade de nome, sinalizando potenciais duplicatas diretamente no item da fila (`ModerationPlaceItemDto.Duplicates`) antes que o moderador confirme a aprovação.

5. **Auditoria de Remoção de Conteúdo em `tb_content_removals`:**
   - Implementação de auditoria formal para remoções e descartes de conteúdo em `tb_content_removals`, documentando a justificativa, responsável e data de expurgo, atendendo às boas práticas de governança e conformidade com diretrizes comunitárias.

---

## Alternativas Consideradas e Descartadas

- **Extensões Espaciais Nativas no Banco (PostGIS):** Descartada nesta fase para evitar dependências pesadas de infraestrutura e complexidade desproporcional de migração no PostgreSQL, uma vez que o escopo territorial é restrito a Sorocaba e o cálculo Haversine puro em C# atende com alto desempenho e precisão métrica.
- **Moderação Síncrona no Momento do Cadastro:** Descartada por degradar a experiência do usuário, travar a requisição HTTP e impedir curadoria humana assíncrona com enriquecimento de dados.
- **Exclusão Física Silenciosa (Hard Delete sem Log):** Descartada por impossibilitar auditoria de abusos, disputas comunitárias e rastreamento de ações de moderadores.

---

## Consequências e Trade-offs

### Positivas (Ganhos)
- Eliminação de duplicidades no catálogo urbano através do alerta preventivo por proximidade métrica.
- Segurança operacional para a equipe de moderação através do mecanismo de reversão em até 24 horas.
- Catálogo público confiável, composto apenas por locais validados por curadoria humana.
- Rastreabilidade total das decisões e exclusões com justificativas auditáveis.

### Negativas / Limitações (Custos aceitos)
- O cálculo de distância Haversine em memória exige o carregamento prévio de coordenadas de candidatos potenciais, demandando evolução futura para índices espaciais caso a base atinja centenas de milhares de pontos.

---

## Referências e Dependências
- [ADR-0001: Fundação da API e Módulo de Usuários](0001-FundacaoEModuloUsuarios.md)
- [ADR-0003: Arquitetura em Camadas por Tipo e Padronização de Responsabilidades](0003-ArquiteturaEmCamadas.md)
