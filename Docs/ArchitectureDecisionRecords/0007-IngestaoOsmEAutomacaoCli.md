# ADR-0007: Ingestão de Dados OSM e Automação via CLI

## Status

Accepted

## Data

2026-10-25

## Rodada / Módulo

07 Ingestão de Dados & Automação CLI

---

## Contexto

Para o lançamento e a viabilidade prática do Soromaps, era fundamental superar o problema da base fria ("cold start"): um mapa urbano colaborativo sem locais cadastrados não gera valor inicial nem incentiva novos usuários a explorarem ou avaliarem a plataforma. Depender unicamente de inserções manuais iniciais seria inviável para cobrir a malha comercial e de lazer da cidade de Sorocaba com rapidez.

Era indispensável conceber uma rotina automatizada, resiliente e auditável para minerar pontos de interesse (POIs) públicos e comerciais de fontes abertas confiáveis, normalizar suas informações para o esquema de dados do Soromaps, categorizá-los adequadamente e integrá-los de forma transparente ao fluxo de moderação existente, com suporte a execuções manuais ou agendadas via linha de comando.

---

## Decisões Tomadas

1. **Mineração Automatizada via OpenStreetMap (Overpass API):**
   - Criação do serviço `OsmPlaceScraperService` com a interface correspondente `IOsmPlaceScraperService`.
   - Utilização da Overpass API pública (`https://overpass-api.de/api/interpreter`) com consulta estruturada delimitada pela *bounding box* geográfica estrita de Sorocaba (`-23.58, -47.55, -23.42, -47.38`).
   - Extração dirigida a nós (*nodes*) e vias (*ways*) contendo tags urbanas relevantes: alimentação (`amenity=cafe|restaurant|bar`), lazer e ar livre (`leisure=park`), cultura e patrimônio (`tourism=museum`, `historic`).

2. **Normalização, Deduplicação e Mapeamento de Categorias:**
   - O serviço realiza a normalização de nomes, extração de centroides geométricos para vias, e mapeia dinamicamente as tags OSM para as categorias correspondentes cadastradas no Soromaps (`Category`).
   - Deduplicação espacial e nominal contra estabelecimentos já existentes na base de dados para evitar duplicidade de registros.
   - Enriquecimento com atributos padrão de conformidade e cabeçalho customizado de `User-Agent` respeitando a política de uso da comunidade OpenStreetMap.

3. **Integração com o Pipeline de Moderação:**
   - Por padrão, todos os locais minerados pelo scraper entram com status `Pending` e `AuthorId = null` (contribuição do sistema), sendo direcionados à fila de moderação para que administradores realizem a curadoria e validação final antes da exibição ao público geral.

4. **Automação Desacoplada via Flags de Entrada CLI no `Program.cs`:**
   - Adicionado tratamento de argumentos de linha de comando no entrypoint `Program.cs` (`--import-places`, `--scrape-places` ou `--import-osm`).
   - Quando acionado com essas flags, a aplicação cria um escopo isolado de serviços, executa o scraper e finaliza o processo sem subir o servidor web HTTP Kestrel.
   - Suporte à flag opcional `--approve`, que permite aprovação imediata dos locais importados em ambientes de testes, desenvolvimento ou migrações controladas.

---

## Alternativas Consideradas e Descartadas

- **Google Places API:** Descartada devido aos altos custos financeiros de requisição em escala, limites estritos de cota e termos de serviço proibitivos quanto ao armazenamento persistente de dados em bancos relacionais proprietários.
- **Web Scraping HTML de Diretórios Locais ou Redes Sociais:** Descartada pela fragilidade dos seletores de interface, risco constante de quebra e incertezas jurídicas quanto aos direitos de extração.
- **Hosted Service Contínuo no Ciclo de Vida da API Web:** Descartada para não atrasar a inicialização do servidor HTTP e não onerar o pool de threads da aplicação web com rotinas demoradas de I/O de rede. O modelo CLI permite agendamento independente (via Cron, Task Scheduler ou pipelines de CI/CD).

---

## Consequências e Trade-offs

### Positivas (Ganhos)
- Catálogo urbano populado imediatamente com centenas de pontos reais e georreferenciados de Sorocaba sem custo de licenciamento de dados (licença aberta ODbL).
- Desacoplamento operacional total entre o servidor web HTTP e os processos de ingestão de dados em lote.
- Manutenção da qualidade do catálogo através do direcionamento seguro para a esteira de moderação já homologada.

### Negativas / Limitações (Custos aceitos)
- Dependência de latência e limites de taxa (*rate limiting*) da infraestrutura pública da Overpass API, exigindo timeouts adequados e cabeçalho identificador.
- Necessidade de curadoria manual humana para refinar nomes informais ou dados cadastrais incompletos herdados do OSM.

---

## Referências e Dependências
- [ADR-0001: Fundação da API e Módulo de Usuários](0001-FundacaoEModuloUsuarios.md)
- [ADR-0003: Arquitetura em Camadas por Tipo e Padronização de Responsabilidades](0003-ArquiteturaEmCamadas.md)
- [ADR-0004: Pipeline de Moderação e Detecção de Duplicatas](0004-PipelineDeModeracaoEDuplicatas.md)
