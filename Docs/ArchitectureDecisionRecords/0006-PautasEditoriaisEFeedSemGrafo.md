# ADR-0006: Pautas Editoriais e Feed Urbano Sem Grafo Social

## Status

Accepted

## Data

2026-10-20

## Rodada / Módulo

06 Feed de Atividades & Pautas Editoriais (Stories)

---

## Contexto

O Soromaps tem como proposta central conectar os moradores aos acontecimentos, novidades e tesouros escondidos de Sorocaba. Para isso, a aplicação requer duas experiências complementares de descoberta: um feed contínuo de atualizações da cidade (novos locais cadastrados, avaliações relevantes, conquistas desbloqueadas) e pautas editoriais curadas com roteiros temáticos ("Stories"), reunindo múltiplos locais com narrativa guiada (ex: "Tour dos Cafés Especiais", "Roteiro Histórico do Além Ponte").

No entanto, a arquitetura tradicional de redes sociais baseada em grafos complexos de conexões bilaterais (amigos, seguidores, solicitações mútuas) exigiria mecanismos custosos de leque de escrita/leitura (*fan-out*), além de dispersar a proposta do produto, que deve ser focada na cidade como um organismo vivo e compartilhado, e não em perfis individuais isolados.

---

## Decisões Tomadas

1. **Feed Urbano Cronológico Unificado Sem Grafo Social:**
   - O feed é projetado em torno do pulso coletivo de Sorocaba (`FeedService` / `ActivityService`), agregando cronologicamente eventos públicos do banco de dados gerados pelas entidades do sistema (`Place`, `Review`, `Achievement`).
   - Elimina-se a necessidade de tabelas de seguidores/seguidos (`followers`/`following`), permitindo que qualquer usuário autenticado ou visitante veja o que há de mais relevante e recente acontecendo na cidade em tempo real.

2. **Silenciamento Customizável por Escopo (`tb_feed_mutes`):**
   - Para garantir controle de relevância sem recorrer a algoritmos opacos de recomendação, foi criada a entidade `tb_feed_mutes` associada ao usuário (`UserId`), permitindo silenciar tópicos por escopo (`ScopeType`: User, Category, Neighborhood, EventType) e valor (`ScopeValue`).
   - O `FeedService` aplica essas preferências diretamente como cláusulas de exclusão nas consultas do feed, garantindo uma linha do tempo personalizada e limpa para cada explorador.

3. **Pautas Editoriais Curadas (`tb_stories` e `tb_story_places`):**
   - Criação da entidade `tb_stories` para roteiros e pautas editoriais com slug amigável, título, subtítulo, bairro focal, textos de introdução/fechamento, status (`Draft`, `Published`, `Archived`) e indicação de origem (`Editorial` ou `AiAssisted` via enum `StoryOrigin`).
   - Associação estruturada com locais via tabela intermediária `tb_story_places`, que define a ordem da parada no roteiro (`sort_order`), título do ângulo de destaque (`angle_title`), texto descritivo contextualizado (`body_text`) e dica local exclusiva (`local_tip`).

4. **Flexibilidade de Publicação de Pautas:**
   - Pautas e roteiros podem ser criados, rascunhados e publicados com ou sem locais vinculados obrigatoriamente no instante inicial, oferecendo flexibilidade para notícias da cidade, notas editoriais rápidas ou roteiros em construção colaborativa.

---

## Alternativas Consideradas e Descartadas

- **Grafo Social Completo (Rede de Seguidores e Timeline Pessoal):** Descartada pela sobrecarga computacional de manutenção de timelines individuais (*fan-out on write/read*), complexidade de infraestrutura de cache e desvio do objetivo primordial do produto, que é o mapa comunitário e a vivência da cidade.
- **CMS Headless Externo para Gestão de Histórias:** Descartada para evitar redundância de infraestrutura, custos adicionais de licenciamento e manter a integridade referencial nativa com a base de dados de locais (`Places`) e autores no PostgreSQL.

---

## Consequências e Trade-offs

### Positivas (Ganhos)
- Arquitetura de feed altamente performática, apoiada em índices cronológicos simples e consultas eficientes.
- Experiência inclusiva: novos usuários têm conteúdo relevante imediato sobre a cidade desde o primeiro acesso, sem a síndrome da "timeline vazia".
- Capacidade editorial rica para curadoria humana e roteiros assistidos por IA que estimulam o turismo e a economia local.
- Controle fino do usuário sobre o que deseja visualizar através do mecanismo de silenciamento granular.

### Negativas / Limitações (Custos aceitos)
- Ausência de uma linha do tempo estritamente pessoal focada em "amigos", assumindo o compromisso de posicionar o Soromaps como um guia comunitário urbano e não uma rede social genérica.

---

## Referências e Dependências
- [ADR-0001: Fundação da API e Módulo de Usuários](0001-FundacaoEModuloUsuarios.md)
- [ADR-0003: Arquitetura em Camadas por Tipo e Padronização de Responsabilidades](0003-ArquiteturaEmCamadas.md)
