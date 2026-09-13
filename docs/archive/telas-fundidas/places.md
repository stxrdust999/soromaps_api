# 🏙️ Explorar lugares

> **Arquivado em 2026-08-17.** A tela virou [`/discover`](../../todo/user/explore.md):
> as duas respondiam à mesma pergunta com critérios diferentes, e critério é
> seção, não rota. O conteúdo abaixo é o documento original, preservado como
> registro — a fronteira com o Descobrir descrita no fim dele deixou de existir.

> Área: usuário público · Rota: `/places` · Status: 🟡 parcial — funciona sobre dados fictícios

## Ideia

O feed de lugares: a tela onde se navega a cidade sem destino definido. Trilhas horizontais sobre a mesma lista de pontos, cada uma com um critério diferente — **Perto de você**, **Em alta**, **Nota máxima da galera**, **Joias escondidas** e **Recém-adicionados** — mais dois eixos de recorte no topo: chips de **categoria** e chips de **vibe**.

O recorte por vibe é a parte que não existe em app de review comum: categoria responde "o que é", vibe responde "pra quê". Um bar e um parque podem servir à mesma noite com a galera, e é assim que as pessoas decidem — não por taxonomia.

## Por que vale

- Ataca a dor nº 1 (descobrir lugares novos) por um caminho mais escaneável que o mapa, que exige saber onde procurar antes de procurar.
- É a página com melhor SEO em potencial: o mapa é um canvas opaco para crawler; cards com nome, categoria e bairro indexam.
- "Joias escondidas" inverte o viés de popularidade que todo agregador tem — dá visibilidade a estabelecimento pequeno, que é a tese do produto.

> Decisões já tomadas e componentes existentes em
> [adr/user/0001-explorar-lugares-sobre-mock.md](../../adr/user/0001-explorar-lugares-sobre-mock.md)

## Dependências para sair do parcial

| O quê | Situação |
|---|---|
| Foto, descrição e categoria no ponto | ❌ colunas ausentes em `markers` — spec em `docs/propostas/2026-08-03-expansao-modelo-ponto.md` |
| `Categoria` como tabela (hoje os chips são constante no front) | ❌ |
| `tags` no modelo do ponto (alimentam vibe e recomendação) | ❌ só existem no mock |
| `Analise` — nota e total de avaliações | ❌ |
| `Visita` — critério real de "Em alta" (hoje é nota, não movimento) | ❌ |
| Geolocalização do usuário para a distância real | ❌ `distancia` é número fixo no mock |
| Paginação / bounding box em `GET /api/markers` | ❌ a rota devolve a tabela inteira |

## Escopo restante

- Trocar os critérios fictícios pelos reais conforme as tabelas nascerem
- Sheet de filtro por `tags`, que os chips não cobrem
- Estado vazio por trilha (hoje a trilha some inteira quando não há item)
- Skeleton com `Suspense`, quando a busca deixar de ser uma chamada só

## Fora do escopo

- Recomendação personalizada — é o [Descobrir](../../todo/user/explore.md)
- Listagem administrativa de pontos, com aprovação — é [Moderação](../../todo/admin/moderation.md)

## Fronteira com as telas vizinhas

- **Home (`/home`)**: o painel sobre o mapa mostra um recorte — "Perto de você" e uma prévia de "Em alta" com link para cá. Proximidade só faz sentido colada ao mapa.
- **[Descobrir](../../todo/user/explore.md) (`/discover`)**: aqui o critério é público e igual para todos; lá é pessoal, derivado do que o usuário visitou.
