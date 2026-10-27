# 🕷️ 11. Crawler e Ingestão OpenStreetMap (OSM)

> **Status:** ✅ Concluído · **Depende de:** [01-SeedEInfraestrutura.md](./01-SeedEInfraestrutura.md), [03-CatalogoELocais.md](./03-CatalogoELocais.md), [04-ModeracaoDeLocais.md](./04-ModeracaoDeLocais.md) · [Índice](./README.md)

Para enriquecer o catálogo de Sorocaba com dados reais sem depender exclusivamente de inserções manuais de exploradores, a API possui um motor autônomo de mineração e ingestão de pontos de interesse (POIs) a partir dos dados abertos do OpenStreetMap (OSM).

---

## ⚙️ 1. Motor de Mineração (`OsmPlaceScraperService`)

O serviço `OsmPlaceScraperService` implementa `IOsmPlaceScraperService` e centraliza o pipeline de extração, higienização, categorização e persistência de locais.

### Funcionalidades do Pipeline
- [x] **Consulta Overpass API:**
  - Endpoint oficial: `https://overpass-api.de/api/interpreter`.
  - Delimitação geográfica estrita do bounding box de Sorocaba (`-23.58, -47.55, -23.42, -47.38`).
  - Mineração de nós (`node`) e vias (`way`) cobrindo amenidades gastronômicas (`cafe`, `restaurant`, `bar`), lazer/parques (`leisure=park`), cultura e patrimônio histórico (`tourism=museum`, `historic`).
- [x] **Fallback Resiliente para Nominatim OSM:**
  - Fallback automático para o serviço oficial Nominatim (`https://nominatim.openstreetmap.org/search`) caso a Overpass API retorne erro HTTP, indisponibilidade ou conjunto vazio.
- [x] **Deduplicação Espacial e Nominal:**
  - Rejeição de duplicatas espaciais baseado na proximidade de coordenadas geográficas (`latitude`, `longitude`) e deduplicação nominal case-insensitive contra registros já persistidos em `tb_places`.
- [x] **Mapeamento Semântico de Categorias:**
  - Varredura de tags OSM (`amenity`, `leisure`, `tourism`, `historic`, `cuisine`) e correspondência com as categorias ativas do sistema (`tb_categories`), com fallback seguro para a categoria padrão ativa de menor ordem.
- [x] **Extração de Metadados e Contatos:**
  - Normalização de dados de endereço (`addr:street`, `addr:housenumber`, `addr:suburb`, `addr:postcode`), telefone (`phone`, `contact:phone`), website (`website`, `contact:website`) e horários (`opening_hours`).
- [x] **Regra de Status e Moderação:**
  - Por padrão, novos locais minerados entram com `status = PlaceStatus.Pending` e `author_id = null`, alimentando a esteira administrativa de moderação (`/admin/moderation`).
  - Suporte à autoaprovação opcional (`PlaceStatus.Approved`) quando explicitamente instruído via parâmetro ou flag.
  - Atualização idempotente e redefinição de status pendente para sincronizações anteriores de origem OSM.

---

## 💻 2. Execução via Linha de Comando (CLI) no `Program.cs`

A ingestão pode ser disparada diretamente pelo terminal sem acoplar ao startup HTTP da API ou expor endpoints públicos desnecessários.

| Flag CLI | Parâmetros Adicionais | Descrição |
|---|---|---|
| `--import-places` | `--approve` (opcional) | Dispara o scraping completo via Overpass/Nominatim e importa para o banco. |
| `--scrape-places` | `--approve` (opcional) | Alias idêntico para a rotina de mineração. |
| `--import-osm` | `--approve` (opcional) | Alias idêntico focado na ingestão do OpenStreetMap. |

### Tarefas
- [x] Registrar `IOsmPlaceScraperService` e `HttpClient` tipado na injeção de dependências (`Program.cs`).
- [x] Tratar os argumentos de linha de comando (`args`) no `Program.cs` antes da inicialização do pipeline HTTP (`app.Run()`).
- [x] Suportar a flag `--approve` para definir o status de importação como `Approved` imediatamente.

---

## ✅ Verificação

- [x] Executar `dotnet run -- --import-osm` consulta o OpenStreetMap e persiste os locais minerados com status `Pending`.
- [x] Executar com a flag `--approve` (`dotnet run -- --import-osm --approve`) cadastra os locais minerados já com status `Approved`.
- [x] Execuções repetidas não duplicam registros já existentes na base de dados.
